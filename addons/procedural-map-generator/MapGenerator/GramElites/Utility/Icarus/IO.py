from os import listdir, getcwd, chdir, path
from os.path import join

def get_levels():
    levels = []
    base_dir = path.dirname(path.abspath(__file__))
    # remove the last two directories to get to the root of the project
    base_dir = path.dirname(base_dir)
    # change the working directory to the root of the project
    base_dir2 = path.dirname(base_dir)

    print(base_dir2)

    levels_dir = path.join(base_dir2, 'vglc_levels', 'Icarus')
    
    for file_name in listdir(levels_dir):
        with open(join(base_dir2, 'vglc_levels', 'Icarus', file_name)) as f:
            levels.append([l.strip() for l in reversed(f.readlines())])

    return levels

def level_to_str(slices):
    return '\n'.join(reversed(slices))
