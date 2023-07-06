using System;
using System.Collections.Generic;
using System.Linq;

namespace MemoryTranser.Scripts.Game.Util {
    public class AliasMethod {
        //https://en.wikipedia.org/wiki/Alias_method
        //https://stackoverflow.com/a/39199014
        //https://www.keithschwarz.com/darts-dice-coins/

        private int _n;
        private float[] _probabilities;
        private int[] _alias;

        public void Constructor(float[] weights) {
            var n = weights.Length;
            var sum = weights.Sum();
            var p = weights.Select(x => x / sum * n).ToArray();

            var prob = new float[n];
            var alias = new int[n];

            Array.Fill(prob, 0f);
            Array.Fill(alias, 0);

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

                prob[l] = p[l];
                alias[l] = g;

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
                prob[g] = 1;
            }

            while (small.Count > 0) {
                var l = small.Dequeue();
                prob[l] = 1;
            }

            _n = n;
            _probabilities = prob;
            _alias = alias;
        }

        public int Roll() {
            var i = new Random().Next(_n);
            return new Random().NextDouble() < _probabilities[i] ? i : _alias[i];
        }
    }
}