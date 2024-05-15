from typing import List

from itertools import repeat
from random import choice

from .IPopulationGenerator import IPopulationGenerator

class NGramPopulationGenerator(IPopulationGenerator):
    __slots__ = ['strand_size', 'gram']

    def __init__(self, n_gram, strand_size):
        self.strand_size = strand_size
        self.gram = n_gram

    def generate(self, n: int) -> List[List[str]]:
        keys = list(self.gram.grammar.keys())
        return [
            self.gram.generate(choice(keys), self.strand_size) 
            for _ in repeat(None, n)
        ]
