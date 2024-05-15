from typing import List
from .IConfig import IConfig

from Optimization.Operators import *
from Utility.Icarus.IO import get_levels, level_to_str
from Utility.Icarus.Behavior import *
from Utility.Icarus.Fitness import *
from Utility import NGram
from Utility.LinkerGeneration import *

class Icarus(IConfig):
    def __init__(self, start_strand_size=80, start_population_size=20, iterations=120) -> None:
        START_STRAND_SIZE = start_strand_size
        START_POPULATION_SIZE = start_population_size
        ITERATIONS = iterations
        n = 2
        self.gram = NGram(n)
        unigram = NGram(1)
        levels = get_levels()
        for level in levels:
            self.gram.add_sequence(level)
            unigram.add_sequence(level)

        unigram_keys = set(unigram.grammar[()].keys())
        pruned = self.gram.fully_connect()     # remove dead ends from grammar
        unigram_keys.difference_update(pruned) # remove any n-gram dead ends from unigram

        mutation_values = list(unigram_keys)
        population_generator = NGramPopulationGenerator(self.gram, START_STRAND_SIZE)

        self.__percent_completable = build_slow_fitness_function(self.gram)
        
        super().__init__(
            start_population_size = START_POPULATION_SIZE,
            iterations = ITERATIONS,
            data_dir = 'IcarusData',
            feature_names = ['density', 'leniency'],
            feature_dimensions = [[0, 1], [0, 1]],
            feature_descriptors = [density, leniency],
            x_label = 'Density',
            y_label = 'Leniency',
            title = '',
            elites_per_bin = 4,
            resolution = 40,
            is_vertical = True,
            start_strand_size = START_STRAND_SIZE,
            max_strand_size = START_STRAND_SIZE,
            minimize_performance = True,
            mutation_values = mutation_values,
            population_generator = population_generator,
            mutate = Mutate(mutation_values, 0.02),
            crossover = SinglePointCrossover(),
            n_mutate = NGramMutate(0.02, self.gram, START_STRAND_SIZE),
            n_crossover = NGramCrossover(self.gram, START_STRAND_SIZE, START_STRAND_SIZE)
        )
    
    def fitness(self, lvl: List[str]) -> float:
        bad_n_grams = self.gram.count_bad_n_grams(lvl)
        return bad_n_grams + 1 - self.__percent_completable(lvl)
    
    def level_to_str(self, lvl: List[str]) -> str:
        return level_to_str(lvl)