from typing import List

from itertools import repeat
from random import choice

from .IPopulationGenerator import IPopulationGenerator

class RandomPopulationGenerator(IPopulationGenerator):
    __slots__ = ['strand_size', 'values']

    def __init__(self, strand_size, values):
        self.strand_size = strand_size
        self.values = values

    def generate(self, n: int) -> List[List[str]]:
        return [
            [choice(self.values) for _ in repeat(None, self.strand_size)]
            for _ in repeat(None, n)
        ]
