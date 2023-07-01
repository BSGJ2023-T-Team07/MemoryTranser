using System;
using System.Collections.Generic;
using System.Linq;

namespace MemoryTranser.Scripts.Game.Util {
    public class AliasMethod {
        private int _n;
        private float[] _probabilities;
        private int[] _alias;

        public void Constructor(float[] weights) {
            _n = weights.Length;
            var sum = weights.Sum();
            var p = weights.Select(x => x / sum).ToArray();

            _probabilities = new float[_n];
            _alias = new int[_n];

            Array.Fill(_probabilities, 1f);
            Array.Fill(_alias, 1);

            var small = new Queue<int>();
            var large = new Queue<int>();

            foreach (var (pp, i) in p.Select((pp, i) => (pp, i))) {
                if (pp < 1) {
                    small.Enqueue(i);
                }
                else {
                    large.Enqueue(i);
                }
            }

            while (small.Count > 0 && large.Count > 0) {
                var l = small.Dequeue();
                var g = large.Dequeue();

                _probabilities[l] = p[l];
                _alias[l] = g;

                p[g] = p[g] + p[l] - 1;

                if (p[g] < 1) {
                    small.Enqueue(g);
                }
                else {
                    large.Enqueue(g);
                }
            }

            while (large.Count > 0) {
                var g = large.Dequeue();
                _probabilities[g] = 1;
            }

            while (small.Count > 0) {
                var l = small.Dequeue();
                _probabilities[l] = 1;
            }
        }

        public int Roll() {
            var i = new Random().Next(_n);
            return new Random().NextDouble() < _probabilities[i] ? i : _alias[i];
        }
    }
}