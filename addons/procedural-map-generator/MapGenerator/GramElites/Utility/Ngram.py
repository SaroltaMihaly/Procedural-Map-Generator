from collections import deque
from random import choices, choice
from itertools import chain

class NGram():
    __slots__ = ['input_size', 'n', 'grammar']

    def __init__(self, size):
        self.input_size = size - 1
        self.n = size
        self.grammar = {}

    def add_sequence(self, sequence):
        queue = deque([], maxlen=self.input_size)

        for token in sequence:
            if len(queue) == queue.maxlen:
                key = tuple(queue)
                if key not in self.grammar:
                    self.grammar[key] = { token: 1 }
                elif token not in self.grammar[key]:
                    self.grammar[key][token] = 1
                else:
                    self.grammar[key][token] += 1

            queue.append(token)

    def has_next_step(self, sequence):
        return tuple(sequence) in self.grammar

    def get_weighted_output(self, sequence):
        unigram = self.grammar[sequence]
        return choices(list(unigram.keys()), weights=unigram.values())[0]

    def get_unweighted_output(self, sequence):
        unigram = self.grammar[sequence]
        return choice(list(unigram.keys()))

    def get_weighted_output_list(self, sequence):
        if sequence not in self.grammar:
            return None

        unigram = self.grammar[sequence]
        keys = list(unigram.keys())
        keys.sort(key=lambda k: -unigram[k])
        return keys

    def get_unweighted_output_list(self, sequence):
        if sequence not in self.grammar:
            return None
            
        unigram = self.grammar[sequence]
        return list(unigram.keys())
    
    def generate(self, prior, size):
        output = []

        while len(output) < size and self.has_next_step(prior):
            new_token = self.get_weighted_output(prior)
            output.append(new_token)
            prior = tuple(prior[1:]) + (new_token,)

        return output

    def sequence_is_possible(self, sequence):
        prior = deque([], maxlen=self.n - 1)

        for token in sequence:
            if len(prior) == prior.maxlen:
                key = tuple(prior)
                if key not in self.grammar:
                    return False

                if token not in self.grammar[key]:
                    return False

            prior.append(token)

        return True
        
    def count_bad_n_grams(self, sequence):
        max_length = self.n - 1
        queue = deque([], maxlen=max_length)
        append_to_queue = queue.append
        bad_transitions = 0

        for token in sequence:
            if len(queue) == max_length:
                input_sequence = tuple(queue)
                if input_sequence not in self.grammar:
                    bad_transitions += 1
                elif token not in self.grammar[input_sequence]:
                    bad_transitions += 1

            append_to_queue(token)

        return bad_transitions

    def fully_connect(self):
        '''
        based on pseudocode: https://en.wikipedia.org/wiki/Tarjan%27s_strongly_connected_components_algorithm
        A vertex is: [index, low_link, on_stack_or_not]
        '''
        groupings = []
        vertices = {}
        s = []
        index = 0

        def tarjan_strong_connect(prior):
            nonlocal index
            vertices[prior] = [index, index, True]
            index += 1
            s.append(prior)

            if prior in self.grammar:
                for new_token in self.grammar[prior]:
                    new_prior = tuple(prior[1:]) + (new_token,)
                    if new_prior not in vertices:
                        tarjan_strong_connect(new_prior)
                        vertices[prior][1] = min(vertices[prior][1], vertices[new_prior][1])
                    elif new_prior in s:
                        vertices[prior][1] = min(vertices[prior][1], vertices[new_prior][0])
                
            if vertices[prior][0] == vertices[prior][1]:
                new_prior = None
                scc_group = []
                while prior != new_prior:
                    new_prior = s.pop()
                    vertices[new_prior][2] = False
                    scc_group.append(new_prior)

                groupings.append(scc_group)

        for prior in self.grammar:
            if prior not in vertices:
                tarjan_strong_connect(prior)

        # remove priors from grammar. We only use the largest group and remove
        # the rest.
        groupings.sort(key=lambda x: -len(x))
        for grp in groupings[1:]:
            for prior in grp:
                if prior in self.grammar:
                    del self.grammar[prior]
                
                for key in self.grammar:
                    to_delete = []
                    for output_token in self.grammar[key]:
                        new_prior = tuple(key[1:]) + (output_token,)
                        if prior == new_prior:
                            to_delete.append(output_token)

                    for output_token in to_delete:
                        del self.grammar[key][output_token]

        # return groups in case dependent grammars want to remove dependencies
        # as well
        return list(chain(*groupings[1:]))