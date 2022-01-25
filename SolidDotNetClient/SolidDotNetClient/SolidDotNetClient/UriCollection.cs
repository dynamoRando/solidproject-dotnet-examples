using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SolidDotNetClient
{
    internal class UriCollection : IEnumerable<Uri>
    {
        #region Private Fields
        private List<Uri> _Uris;
        #endregion

        #region Public Properties
        public int Count => _Uris.Count;
        public bool IsReadOnly => false;
        public List<Uri> List => _Uris;
        #endregion

        #region Constructors
        public UriCollection()
        {
            _Uris = new List<Uri>();
        }

        public UriCollection(int size)
        {
            _Uris = new List<Uri>(size);
        }
        #endregion

        #region Public Methods
        public Uri Get(string folder)
        {
            if (!folder.EndsWith("/"))
            {
                folder = folder + "/";
            }

            if (!folder.StartsWith("/"))
            {
                folder = "/" + folder;
            }

            foreach (var Uri in _Uris)
            {
                if (string.Equals(Uri.AbsolutePath, folder, StringComparison.OrdinalIgnoreCase))
                {
                    return Uri;
                }
            }

            return null;
        }

        public void Add(Uri item)
        {
            if (!Contains(item.AbsolutePath))
            {
                _Uris.Add(item);
            }
            else
            {
                throw new InvalidOperationException(
                  $"There is already a Uri folder: {item.AbsolutePath}");
            }
        }

        public bool Contains(Uri Uri)
        {
            foreach (var x in _Uris)
            {
                if (string.Equals(x.AbsolutePath, Uri.AbsolutePath, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Contains(string folder)
        {
            if (!folder.EndsWith("/"))
            {
                folder = folder + "/";
            }

            if (!folder.StartsWith("/"))
            {
                folder = "/" + folder;
            }

            foreach (var Uri in _Uris)
            {
                if (string.Equals(Uri.AbsolutePath, folder, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Remove(Uri item)
        {
            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _Uris.Count; i++)
            {
                Uri currentUri = _Uris[i];

                if (string.Equals(currentUri.AbsolutePath, item.AbsolutePath, StringComparison.OrdinalIgnoreCase))
                {
                    _Uris.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public bool Remove(string folder)
        {
            if (!folder.EndsWith("/"))
            {
                folder = folder + "/";
            }

            if (!folder.StartsWith("/"))
            {
                folder = "/" + folder;
            }

            bool result = false;

            // Iterate the inner collection to
            // find the box to be removed.
            for (int i = 0; i < _Uris.Count; i++)
            {
                Uri currentUri = _Uris[i];

                if (string.Equals(currentUri.AbsolutePath, folder, StringComparison.OrdinalIgnoreCase))
                {
                    _Uris.RemoveAt(i);
                    result = true;
                    break;
                }
            }
            return result;
        }

        public void Clear()
        {
            _Uris.Clear();
        }

        public void AddRange(List<Uri> uris)
        {
            _Uris.AddRange(uris);
        }

        public Uri this[int index]
        {
            get { return _Uris[index]; }
            set { _Uris[index] = value; }
        }

        public IEnumerator<Uri> GetEnumerator()
        {
            return new UriEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new UriEnumerator(this);
        }
        #endregion

        #region Private Methods
        #endregion


    }
}
