def say_goodbye():
    print("Goodbye!")

def write_matrix(length):
    matrix = []
    for i in range(length):
        row = []
        for j in range(length):
            row.append(j)
        matrix.append(row)
    for row in matrix:
        print(row)
    return matrix

write_matrix(6)
