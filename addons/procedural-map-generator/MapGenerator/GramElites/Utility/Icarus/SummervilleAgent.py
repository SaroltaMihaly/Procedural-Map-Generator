'''
modified directly from: 
    1. https://github.com/TheVGLC/TheVGLC/blob/master/PlatformerPathfinding/pathfinding.py
    2. https://github.com/TheVGLC/TheVGLC/blob/master/PlatformerPathfinding/test_level.py
'''

from heapq import heappush, heappop
from .config import JUMPS, SOLIDS

DEBUG_DISPLAY = False

def isSolid(tile):
    return tile in SOLIDS

# should account for level edges and wrap
def levelTile(levelStr, x, y):
    maxX = len(levelStr[0])-1
    maxY = len(levelStr)-1

    if y < 0:
        y = 0

    if y > maxY:
        return None, None

    while x < 0:
        x += (maxX + 1)
        
    while x > maxX:
        x -= (maxX + 1)

    return levelStr[y][x], (x, y)

def makeGetNeighbors(jumps,levelStr,visited,isSolid):
    jumpDiffs = []
    for jump in jumps:
        jumpDiff = [jump[0]]
        for ii in range(1,len(jump)):
            jumpDiff.append((jump[ii][0]-jump[ii-1][0],jump[ii][1]-jump[ii-1][1]))
        jumpDiffs.append(jumpDiff)
    jumps = jumpDiffs
    
    def getNeighbors(pos):
        dist = pos[0]-pos[2]
        pos = pos[1]
        visited.add((pos[0],pos[1]))
        below = (pos[0],pos[1]+1)
        neighbors = []

        tile_current, tile_current_pos = levelTile(levelStr, pos[0], pos[1])
        if tile_current is None:
            return []

        if pos[2] != -1:
            ii = pos[3] +1
            jump = pos[2]
            if ii < len(jumps[jump]):
                tile_jump, tile_jump_pos = levelTile(levelStr, pos[0]+pos[4]*jumps[jump][ii][0], pos[1]+jumps[jump][ii][1])
                if tile_jump is not None and not isSolid(tile_jump):
                    neighbors.append([dist+1,(tile_jump_pos[0], tile_jump_pos[1], jump, ii, pos[4])])

        tile_below, tile_below_pos = levelTile(levelStr, below[0], below[1])
        if tile_below is None:
            return []

        if isSolid(tile_below):
            tile_right, tile_right_pos = levelTile(levelStr, pos[0]+1, pos[1])
            if tile_right is not None and not isSolid(tile_right):
                neighbors.append([dist+1,(tile_right_pos[0], tile_right_pos[1], -1)])

            tile_left, tile_left_pos = levelTile(levelStr, pos[0]-1, pos[1])
            if tile_left is not None and not isSolid(tile_left):
                neighbors.append([dist+1,(tile_left_pos[0], tile_left_pos[1], -1)])

            for jump in range(len(jumps)):
                ii = 0

                tile_jump_right, tile_jump_right_pos = levelTile(levelStr, pos[0]+jumps[jump][ii][0], pos[1]+jumps[jump][ii][1])
                if tile_jump_right is not None and not isSolid(tile_jump_right):
                    neighbors.append([dist+ii+1,(tile_jump_right_pos[0], tile_jump_right_pos[1], jump, ii, 1)])

                tile_jump_left, tile_jump_left_pos = levelTile(levelStr, pos[0]-jumps[jump][ii][0], pos[1]+jumps[jump][ii][1])
                if tile_jump_left is not None and not isSolid(tile_jump_left):
                    neighbors.append([dist+ii+1,(tile_jump_left_pos[0], tile_jump_left_pos[1], jump, ii, 1)])

        else:
            neighbors.append([dist+1,(tile_below_pos[0], tile_below_pos[1], -1)])

            tile_below_right, tile_below_right_pos = levelTile(levelStr, pos[0]+1, pos[1]+1)
            if tile_below_right is not None and not isSolid(tile_below_right):
                neighbors.append([dist+1.4,(tile_below_right_pos[0], tile_below_right_pos[1], -1)])

            tile_below_left, tile_below_left_pos = levelTile(levelStr, pos[0]-1, pos[1]+1)
            if tile_below_left is not None and not isSolid(tile_below_left):
                neighbors.append([dist+1.4,(tile_below_left_pos[0], tile_below_left_pos[1], -1)])

        if DEBUG_DISPLAY:
            print()
            print('neighbors', pos, neighbors)
            print()

        return neighbors
    return getNeighbors

def percent_completable(src, levelStr):
    visited = set()
    getNeighbors = makeGetNeighbors(JUMPS,levelStr,visited,isSolid)
    maxY = len(levelStr)-1
    bestY = 0
    heuristic = lambda pos: abs(bestY-pos[1])

    dist = {}
    prev = {}
    dist[src] = 0
    prev[src] = None
    heap = [(dist[src], src, 0)]
    furthest_y = maxY

    if DEBUG_DISPLAY:
        import sys
        explored = set()
        path = set()

        def displayLevel():
            print()
            for yy, row in enumerate(levelStr):
                for xx, tile in enumerate(row):
                    if (xx, yy) in path:
                        sys.stdout.write('*')
                    elif (xx, yy) in explored:
                        sys.stdout.write('.')
                    else:
                        sys.stdout.write(tile)
                sys.stdout.write('\n')

    while heap:
        node = heappop(heap)

        if DEBUG_DISPLAY:
            explored.add((node[1][0], node[1][1]))
            displayLevel()
            
        for next_node in getNeighbors(node):
            next_node[0] += heuristic(next_node[1])
            next_node.append(heuristic(next_node[1]))
            if next_node[1] not in dist or next_node[0] < dist[next_node[1]]:
                current_y = next_node[1][1]
                furthest_y = min(furthest_y, current_y)
                if furthest_y == bestY:

                    if DEBUG_DISPLAY:
                        full_path = []
                        path_node = next_node[1]

                        while path_node != None:
                            path.add((path_node[0], path_node[1]))
                            full_path.append(path_node)
                            
                            if path_node == next_node[1]:
                                path_node = node[1]
                            else:
                                path_node = prev[path_node]

                        print('path', list(reversed(full_path)))
                        displayLevel()
                        
                    break
                
                dist[next_node[1]] = next_node[0]
                prev[next_node[1]] = node[1]
                heappush(heap, next_node)

        if furthest_y == bestY:
            break

    if DEBUG_DISPLAY:
        import sys
        sys.exit(-1)

    return (maxY - furthest_y) / maxY
