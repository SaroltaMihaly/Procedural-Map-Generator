from shutil import rmtree
from os.path import isdir, join
from os import mkdir

from . import columns_into_grid_string
from . import update_progress

def clear_directory(directory_name):
    if not isdir(directory_name):
        mkdir(directory_name)
    else:
        rmtree(directory_name)
        mkdir(directory_name)

def make_if_does_not_exist(directory_name):
    if not isdir(directory_name):
        mkdir(directory_name)


def save_map_elites_to_dir(bins, directory_name):
    keys = list(bins.keys())
    for progress, key in enumerate(keys):
        for index, entry in enumerate(bins[key]):
            level = entry[1]

            level_file = open(join(directory_name, f'{key[1]}_{key[0]}_{index}.txt'), 'w')
            level_file.write(columns_into_grid_string(level))
            level_file.close()

        update_progress(progress / len(keys)) 
    update_progress(1)
    