from collections import deque
from Utility.Math import rmse

# - exhaustive search, use a flag and life is good
# - if path has better characteristics, run agent else continue
# - change to stamina branch of DG, how many iterations to run for DG. Add third dimension if possible.
# - what % of the links asked to make work versus do not
# - % that the segments could be completed
# - what happens if a walkthrough fails? Pick x segments, if linking fails then
#   pick new segments. Report on how many times a link failed to find a level.
# - Seth makes overleaf
# - write the paper
def generate_link(grammar, start, end, additional_columns, agent=None, feature_descriptors=None, max_length=40):
    '''
    Build a link between two connecting segments. If an agent and feature descriptors
    are provided then this will exhaustively search all possible links and use the
    one with the lowest rmse between the feature values found and the targets based
    on the start and end level segments provided in the arguments.

    Based off of: https://www.redblobgames.com/pathfinding/a-star/introduction.html

    :param NGram grammar: grammar learned from training levels.
    :param [str] start: start level segment that link begins from.
    :param [str] end: end level segment that link connects to.
    :param int additional_columns: number of extra columns to be generated.
    :param function agent: calculates value [0,1] for percent that an agent 
    can complete a given level.
    :param [function] feature_descriptors: list of functions that evaluate a 
    level segment based on some behavioral characteristic.
    :param int max_length: maximum length of a link between start and end.
    :return: the connecting link or None if no valid link found.
    :rtype: [str]
    '''
    assert grammar.sequence_is_possible(start)
    assert grammar.sequence_is_possible(end)

    if agent != None:
        assert agent(start) == 1.0
        assert agent(end) == 1.0
    
    if feature_descriptors != None:
        assert type(feature_descriptors) == list
        assert len(feature_descriptors) > 0

    # if no extra columns have to be generated and the combination of start and
    # end is already valid, then return. However, if an agent is given, validate
    # that the agent can play through the combination before returning.
    if additional_columns == 0 and grammar.sequence_is_possible(start + end):
        if agent == None:
            return []
        elif agent(start + end) == 1.0:
            return []

    # if we have been given an agent and feature descriptors than calculate
    # the average behavioral charecteristics of the two level segments to
    # find a target
    exhaustive_search = agent != None and feature_descriptors != None
    if exhaustive_search:
        target_bc = [(bc(start) + bc(end))/2 for bc in feature_descriptors]
        best_rmse = 1000
        best_link = None

    # generate path of minimum length with an n-gram
    start_link = grammar.generate(tuple(start), additional_columns)
    min_path = start + start_link

    # BFS to find the ending prior
    queue = deque()
    came_from = {}

    start_node = (tuple(min_path[-(grammar.n - 1):]), 0)
    end_prior = tuple(end[:grammar.n - 1])
    queue.append(start_node)
    path = None

    # loop through queue until a path is found
    while queue:
        node = queue.popleft()
        if node[1] + 1 > max_length:
            continue
            
        current_prior = node[0]
        output = grammar.get_unweighted_output_list(current_prior)
        if output == None:
            continue

        for new_column in output:
            # build the new prior with the slice found by removing the first 
            # slice
            new_prior = current_prior[1:] + (new_column,)
            new_node = (new_prior, node[1] + 1)

            if new_node in came_from:
                continue

            # if the prior is not the end prior, add it to the search queue and
            # continue the search
            came_from[new_node] = node
            queue.append(new_node)
            if new_prior != end_prior:
                continue

            # reconstruct path
            path = []
            temp_node = new_node
            while temp_node != start_node:
                path.insert(0, temp_node[0][-1])
                temp_node = came_from[temp_node]

            # only use the path if we have constructed a path that is larger 
            # than n.
            if len(path) >= grammar.n:
                link = start_link + path[:-(grammar.n - 1)]

                if exhaustive_search:
                    _rmse = rmse(target_bc, [bc(link) for bc in feature_descriptors])
                    if _rmse < best_rmse:
                        playability = agent(start + link + end)
                        if playability == 1.0:
                            best_link = link
                            best_rmse = _rmse
                else:
                    return link
            
    if exhaustive_search:
        return best_link

    # No link found
    return None