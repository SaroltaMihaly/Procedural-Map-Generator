from Config import Icarus
import Pypy3_Tasks

from time import time
import argparse

start = time()

parser = argparse.ArgumentParser(description='Gram-Elites')
parser.add_argument('--seed', type=int, default=0, help='Set seed for generation')
parser.add_argument('--start_strand_size', type=int, default=80, help='Set start strand size for the genetic algorithm')
parser.add_argument('--start_population_size', type=int, default=20, help='Set the start population size for the genetic algorithm')
parser.add_argument('--iterations', type=int, default=120, help='Set the number of iterations for the genetic algorithm')
# parser.add_argument(
#     '--runs', 
#     type=int,
#     default=10,
#     help='Set the # of runs for --average-generated.')
# parser.add_argument('--segments', type=int, default=3, help='set # of segments to be combined.')

args = parser.parse_args()

config = Icarus(start_strand_size=args.start_strand_size, start_population_size=args.start_population_size, iterations=args.iterations)
print('Algorithm => Gram-elites')
config.crossover = config.n_crossover
config.mutate = config.n_mutate
alg_type = 'gram_elites'

Pypy3_Tasks.GenerateCorpus(config, alg_type).run(args.seed)

end = time()

hours, rem = divmod(end-start, 3600)
minutes, seconds = divmod(rem, 60)
print("{:0>2}:{:0>2}:{:05.2f}".format(int(hours),int(minutes),seconds))

