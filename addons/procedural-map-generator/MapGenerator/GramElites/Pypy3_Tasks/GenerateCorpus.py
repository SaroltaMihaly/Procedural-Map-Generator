
from Utility.GridTools import columns_into_grid_string
from Optimization import MapElites
from Utility.Math import *
from Config import IConfig
from Utility import *

from subprocess import call
from os.path import join
from os import listdir, getcwd, chdir, path
from csv import writer
import json

class GenerateCorpus():
    def __init__(self, config: IConfig, alg_type: str):
        self.config = config
        self.alg_type = alg_type

    def run(self, seed):
        #######################################################################
        print(self.config.data_dir)
        base_dir = path.dirname(path.abspath(__file__))
        base_dir = path.dirname(base_dir)

        DIR = join(base_dir, self.config.data_dir)
        make_if_does_not_exist(DIR)

        ALG_DATA_DIR = join(DIR, self.alg_type)
        LEVEL_DIR = join(ALG_DATA_DIR, 'levels')
        
        clear_directory(ALG_DATA_DIR)
        make_if_does_not_exist(LEVEL_DIR)

        #######################################################################
        # map_elites_config = join(data_dir, 'config_map_elites')
        # data_file = join(data_dir, 'data')
        DATA_FILE = join(ALG_DATA_DIR, 'config_map_elites_generate_corpus_data.csv')

        print('writing config file for graphing')
        config = {
            'data_file': DATA_FILE,
            'x_label': self.config.x_label,
            'y_label': self.config.y_label,
            'save_file': join(ALG_DATA_DIR, 'map_elites.pdf'),
            'title': self.config.title,
            'resolution': self.config.resolution,
            'feature_dimensions': self.config.feature_dimensions
        }

        f = open(join(ALG_DATA_DIR, 'generate_corpus_config.json'), 'w')
        f.write(json.dumps(config, indent=2))
        f.close()

        #######################################################################
        print('Generating a corpus...')
        gram_search = MapElites(
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
            rng_seed=seed
        )
        gram_search.run(self.config.iterations)

        #######################################################################
        print('Validating levels...')
        valid_levels = 0
        invalid_levels =  0
        fitnesses = {}
        bins = gram_search.bins
        f = open(DATA_FILE, 'w')
        csv_writer = writer(f)
        csv_writer.writerow(self.config.feature_names + ['index', 'performance'])
        
        keys = list(bins.keys())
        for progress, key in enumerate(keys):
            for index, entry in enumerate(bins[key]):
                fitness = entry[0]
                level = entry[1]

                if fitness == 0.0:
                    valid_levels += 1
                else:
                    invalid_levels +=1 
                
                csv_writer.writerow(list(key) + [index, fitness])

                file_name = f'{key[1]}_{key[0]}_{index}.txt'
                level_file = open(join(LEVEL_DIR, file_name), 'w')
                level_file.write(self.config.level_to_str(level))
                level_file.close()

                fitnesses[file_name] = fitness

            update_progress(progress / len(keys)) 

        f.close()

        results = {
            'valid_levels': valid_levels,
            'invalid_levels': invalid_levels,
            'total_levels': valid_levels + invalid_levels,
            'fitness': fitnesses,
        }

        update_progress(1)

        #######################################################################
        print('Storing results and Generating MAP-Elites graph...')
        f = open(join(ALG_DATA_DIR, 'generate_corpus_info.json'), 'w')
        f.write(json.dumps(results, indent=2))
        f.close()
        
        # call(['python3', join('Scripts', 'build_map_elites.py'), self.config.data_dir, str(self.config.elites_per_bin)])
        print('Done!')