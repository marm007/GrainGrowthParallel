using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GrainGrowthServer
{
    public static class StackExtensions
    {
        static Random rnd = new Random();

        public static Stack Shuffle(this Stack stack)
        {
            var values = stack.ToArray();
            Stack toReturn = new Stack();

            foreach (var value in values.OrderBy(x => rnd.Next()))
                toReturn.Push(value);
            return toReturn;
        }

        public static List<Stack> ShuffleList(this Stack stack, int number)
        {
            var values = stack.ToArray();
            List<Stack> stacks = new List<Stack>();
            for (int i = 0; i < number; i++)
                stacks.Add(new Stack());

            int index = 0;
            int max = stack.Count / number;

            foreach (var value in values.OrderBy(x => rnd.Next()))
            {
                int mod = index / max;
                if (mod >= number)
                    mod = number - 1;
                stacks[mod].Push(value);
                index++;
            }
         
            return stacks;
        }
    }
}
