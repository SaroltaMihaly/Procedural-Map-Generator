from random import randrange

class SinglePointCrossover:
    def operate(self, parent_1, parent_2):
        cross_over_point = randrange(1, min(len(parent_1), len(parent_2)) - 1)

        return [
            parent_1[:cross_over_point] + parent_2[cross_over_point:],
            parent_2[:cross_over_point] + parent_1[cross_over_point:]
        ]
