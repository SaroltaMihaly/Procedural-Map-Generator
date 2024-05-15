import sys

def update_progress(progress, message=None):
    '''
    modifed from: https://stackoverflow.com/questions/3160699/python-progress-bar
    NOTE: tqdm is better but avoiding dependencies for pypy
    '''
    barLength = 20 # Modify this to change the length of the progress bar
    status = ""
    if isinstance(progress, int):
        progress = float(progress)
    
    if not isinstance(progress, float):
        progress = 0
        status = "error: progress var must be float\r\n"
    elif progress < 0:
        progress = 0
        status = "Halt...\r\n"
    elif progress >= 1:
        progress = 1
        status = "Done\r\n"

    block = int(round(barLength*progress))

    if message != None:
        text = f'\rPercent [{"#"*block + "-"*(barLength-block)}] {round(progress*100, 2)}% {status} :: {message}'
    else:
        text = f'\rPercent [{"#"*block + "-"*(barLength-block)}] {round(progress*100, 2)}% {status}'
        
    sys.stdout.write(text)
    sys.stdout.flush()

class Bar:
    def __init__(self, denominator):
        self.numerator = -1
        self.denominator = denominator
        self.update()

    def update(self, message=None):
        self.numerator += 1
        if self.numerator >= self.denominator:
            update_progress(1)

        update_progress(self.numerator / self.denominator, message=message)

    def done(self):
        update_progress(1)
