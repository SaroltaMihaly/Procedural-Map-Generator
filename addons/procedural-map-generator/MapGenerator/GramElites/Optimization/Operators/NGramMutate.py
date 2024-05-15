from typing import List

from Utility.LinkerGeneration import generate_link
from random import randrange, random
from .IMutate import IMutate

class NGramMutate(IMutate):
    __slots__ = [
        'standard_deviation', 'mutation_values', 'mutation_rate', 
        'max_length', 'gram']

    def __init__(self, mutation_rate, gram, max_length):
        self.mutation_rate = mutation_rate
        self.max_length = max_length
        self.gram = gram

    def mutate(self, strand: List[str]) -> List[str]:
        if strand == None:
            return None

        if random() < self.mutation_rate:
            point = randrange(self.gram.n - 1, len(strand) - self.gram.n - 1)
            start =  strand[:point]
            end = strand[point + 1:]

            assert self.gram.sequence_is_possible(start)
            assert self.gram.sequence_is_possible(end)
            
            link = generate_link(self.gram, start, end, 1)
            path = start + link + end
            
            assert self.gram.sequence_is_possible(path)

            return path[:self.max_length]
        
        return strand
        