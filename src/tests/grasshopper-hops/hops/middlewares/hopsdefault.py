import hops.base as base


class HopsDefault(base.HopsBase):
    def __init__(self):
        super(HopsDefault, self).__init__(None)

    def start(self, address, port):
        pass
