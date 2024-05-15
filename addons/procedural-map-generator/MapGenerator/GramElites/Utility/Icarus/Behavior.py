from .config import *

def density(slices):
    num_count_blocks = 0
    total_number_of_blocks = 0
    for sl in slices:
        for bl in sl:
            if bl in SOLIDS:
                num_count_blocks += 1
        total_number_of_blocks += len(sl)

    return num_count_blocks / total_number_of_blocks

def leniency(slices):
    count = 0
    for sl in slices:
        if HAZARD in sl:
            count += 1/3
        if DOOR in sl:
            count += 1/3
        if MOVING in sl:
            count += 1/3

    return count / len(slices)
