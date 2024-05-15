from typing import List, Callable
from Optimization.Operators import IMutate, ICrossover, IPopulationGenerator

class IConfig:
    def __init__(
        self, start_population_size: int, iterations: int, data_dir: str, 
        feature_names: List[str], feature_dimensions: List[str],
        feature_descriptors: List[Callable[[List[str]], float]], x_label: str,
        y_label: str, title: str, elites_per_bin: int, resolution: int, 
        is_vertical: bool, start_strand_size: int, max_strand_size: int,
        minimize_performance: bool,  mutation_values: List[str], 
        population_generator: IPopulationGenerator, mutate: IMutate, 
        crossover: ICrossover, n_mutate: IMutate, n_crossover: ICrossover
    ):
        self.start_population_size = start_population_size
        self.iterations = iterations
        self.data_dir = data_dir
        self.feature_names = feature_names
        self.feature_dimensions = feature_dimensions
        self.feature_descriptors = feature_descriptors
        self.x_label = x_label
        self.y_label = y_label
        self.title = title
        self.elites_per_bin = elites_per_bin
        self.resolution = resolution
        self.is_vertical = is_vertical
        self.start_strand_size = start_strand_size
        self.max_strand_size = max_strand_size
        self.minimize_performance = minimize_performance
        self.mutation_values = mutation_values
        self.population_generator = population_generator
        self.mutate = mutate
        self.crossover = crossover
        self.n_mutate = n_mutate
        self.n_crossover = n_crossover

    def fitness(self, lvl: List[str]) -> float:
        raise NotImplementedError()
    
    def level_to_str(self, lvl: List[str]) -> str:
        raise NotImplementedError()
