using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntryPoint{
    interface ITree<Vector2>{
        bool isEmpty { get; }
        Vector2 value { get; }
        bool evenForY { get; set; }
        ITree<Vector2> left { get; }
        ITree<Vector2> right { get; }
    }

    class Empty<Vector2> : ITree<Vector2>{
        public bool evenForY { get; set; }

        public bool isEmpty{
            get{
                return true;
            }
        }

        public ITree<Vector2> left{
            get{
                throw new NotImplementedException();
            }
        }

        public ITree<Vector2> right{
            get{
                throw new NotImplementedException();
            }
        }

        public Vector2 value{
            get{
                throw new NotImplementedException();
            }
        }
    }

    class Node<Vector2> : ITree<Vector2>{
        public bool evenForY{ get; set; }
        public bool isEmpty{
            get{
                return false;
            }
        }

        public ITree<Vector2> left { get; set; }

        public ITree<Vector2> right { get; set; }

        public Vector2 value { get; set; }

        public Node(ITree<Vector2> l, Vector2 v, ITree<Vector2> r, bool x){
            value = v;
            left = l;
            right = r;
            evenForY = x;
        }
    }
}
