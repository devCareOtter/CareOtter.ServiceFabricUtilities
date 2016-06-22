namespace CareOtter.ServiceFabricUtilities.Paging
{
    internal class PagingIdManager
    {
        private readonly bool[] _ids;
        private ushort _pos = 0;

        private object thisLock = new object();

        public PagingIdManager()
        {
            _ids = new bool[ushort.MaxValue+1];
        }

        public ushort GetId()
        {
            lock (thisLock)
            {
                unchecked //this uses int overflow as a feature
                {
                    while (_ids[_pos++]) ;

                    _ids[_pos] = true;
                    return _pos;
                }
            }
        }

        public void FreeId(ushort id)
        {
            lock (thisLock)
            {
                _ids[id] = false;
            }
        }
    }
}