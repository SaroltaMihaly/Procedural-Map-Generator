from typing import List, Tuple

class ICrossover:
    def operate(self, parent_1: List[str], parent_2: List[str]) -> Tuple[List[str], List[str]]:
        raise NotImplementedError