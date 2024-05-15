from typing import List

from random import choice, randrange, random
from .IMutate import IMutate

class Mutate(IMutate):
    __slots__ = ['mutation_values', 'mutation_rate']

    def __init__(self, mutation_values, mutation_rate):
        self.mutation_values = mutation_values
        self.mutation_rate = mutation_rate

    # Strand is modified in memory, not copied. It's easier with the n-gram
    # version.
    def mutate(self, strand: List[str]) -> List[str]:
        if random() < self.mutation_rate:
            strand[randrange(0, len(strand))] = choice(self.mutation_values)
        
        return strand
