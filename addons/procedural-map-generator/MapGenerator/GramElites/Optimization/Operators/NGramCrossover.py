from Utility.LinkerGeneration import generate_link
from random import randrange

class NGramCrossover:
    __slots__ = ['gram', 'min_length', 'max_length', 'max_attempts']

    def __init__(self, gram, min_length, max_length, max_attempts=10):
        self.max_attempts = max_attempts
        self.max_length = max_length
        self.min_length = min_length
        self.gram = gram

    def operate(self, parent_1, parent_2):
        
        # get crossover point based on strand lengths. Note that there must be
        # at least n-1 columns on either side for a valid prior to be built. 
        strand_size = min(len(parent_1), len(parent_2))
        cross_over_point = randrange(self.gram.n - 1, strand_size - self.gram.n - 1)

        # Built first level. This operation assumes we are working with a fully-
        # connected n-gram. Otherwise, BFS is not guranteed to find a path between
        # two random but valid priors.
        start = parent_1[:cross_over_point]
        end = parent_2[cross_over_point:]
        p_1 = start + generate_link(self.gram, start, end, 0) + end
        assert self.gram.sequence_is_possible(p_1)

        # build second level
        start = parent_2[:cross_over_point]
        end = parent_1[cross_over_point:]
        assert self.gram.sequence_is_possible(start)
        assert self.gram.sequence_is_possible(end)
        p_2 = start + generate_link(self.gram, start, end, 0) + end
        assert self.gram.sequence_is_possible(p_2)

        # return truncated results
        return p_1[:self.max_length], p_2[:self.max_length]
