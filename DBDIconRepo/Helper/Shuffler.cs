﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDIconRepo.Helper
{
    public static class Shuffler
    {
        private static Random _randomizer = new();

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            return source.OrderBy((item) => _randomizer.Next());
        }
    }
}
