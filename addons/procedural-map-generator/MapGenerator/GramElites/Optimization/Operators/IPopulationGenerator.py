from typing import List

class IPopulationGenerator:
    def generate(self, n: int) -> List[List[str]]:
        raise NotImplementedError()