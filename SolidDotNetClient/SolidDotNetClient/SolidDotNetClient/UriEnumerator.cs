using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;


namespace SolidDotNetClient
{
    internal class UriEnumerator : IEnumerator<Uri>
    {
        private UriCollection _Uris;
        private int _index;
        private Uri _current;

        public Uri Current => _current;

        object IEnumerator.Current => Current;

        public UriEnumerator(UriCollection collection)
        {
            _Uris = collection;
            _index = -1;
            _current = default(Uri);
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            //Avoids going beyond the end of the collection.
            if (++_index >= _Uris.Count)
            {
                return false;
            }
            else
            {
                // Set current box to next item in collection.
                _current = _Uris[_index];
            }
            return true;
        }

        public void Reset()
        {
            _index = -1;
        }
    }
}
