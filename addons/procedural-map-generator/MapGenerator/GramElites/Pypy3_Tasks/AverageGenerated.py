from Optimization import MapElites
from Utility import *

from json import dumps as json_dumps

class AverageGenerated:
    def __init__(self, config, alg_type, seed):
        self.config = config
        self.alg_type = alg_type
        self.seed = seed

    def run_generator(self, runs, build_map_elites, iterations, level_path):
        bins = {}
        counts = []

        for i in range(runs):
            print(f'{i+1}/{runs}')

            search = build_map_elites(i)
            counts.append(search.run(iterations))

            for k in search.bins:
                if search.bins[k][0][0] == 0.0:
                    key = str(k)
                    if key in bins:
                        bins[key] += 1
                    else:
                        bins[key] = 1

            print('Storing levels...')
            levels_dir = join(level_path, f'run_{i}')
            clear_directory(levels_dir)
            save_map_elites_to_dir(search.bins, levels_dir)
            print()     
        
        return bins, counts, search

    def run(self, runs):
        #######################################################################
        print('Setting up data directory...')

        ALG_DATA_DIR = join(self.config.data_dir, self.alg_type)
        LEVEL_DIR = join(ALG_DATA_DIR, 'average_generated_levels')
        
        make_if_does_not_exist(self.config.data_dir)
        make_if_does_not_exist(ALG_DATA_DIR)
        clear_directory(LEVEL_DIR)

        #######################################################################
        print('Running Gram-Elites')
        builder = lambda i: MapElites(
            self.config.start_population_size,
            self.config.feature_descriptors,
            self.config.feature_dimensions,
            self.config.resolution,
            self.config.fitness,
            self.config.minimize_performance,
            self.config.population_generator,
            self.config.mutate,
            self.config.crossover,
            self.config.elites_per_bin,
            rng_seed=self.seed + i
        )

        ng_bins, ng_counts, ng_search  = self.run_generator(
            runs, 
            builder, 
            self.config.iterations, 
            LEVEL_DIR
        )
        #######################################################################
        print('Saving Results and Bins...')
        f = open(join(ALG_DATA_DIR, 'bins.json'), 'w')
        f.write(json_dumps({
            'runs': runs,
            'resolution': self.config.resolution,
            'x_label': self.config.feature_names[0],
            'y_label': self.config.feature_names[1],
            'bins': ng_bins,
        }, indent=2))
        f.close()

        print('Saving results')
        f = open(join(ALG_DATA_DIR, 'counts.json'), 'w')
        f.write(json_dumps(ng_counts, indent=2))
        f.close()

        #######################################################################
        # print('Starting Plotters...')
        # call(['python3', join('Scripts', 'build_counts_graph.py'), self.config.data_dir, str(self.config.start_population_size)])
