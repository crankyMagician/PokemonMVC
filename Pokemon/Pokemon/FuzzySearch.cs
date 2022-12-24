using System;
using System.Collections.Generic;

namespace Pokemon
{
    /*
     * I created a static class to implement the fuzzy search 
     */
    public static class FuzzySearch
    {
        
        // Computes the distance between two strings using the Levenshtein distance algorithm
        public static int ComputeDistance(string s, string t)
        {
            // Get the lengths of the strings
            int n = s.Length;
            int m = t.Length;

            // Create a 2D array to store the distance values
            int[,] d = new int[n + 1, m + 1];

            // Check if either string is empty
            if (n == 0)
            {
                // Return the length of the other string
                return m;
            }
            if (m == 0)
            {
                // Return the length of the other string
                return n;
            }

            // Initialize the distance array with the lengths of the input strings
            for (int i = 0; i <= n; d[i, 0] = i++)
            {
            }
            for (int j = 0; j <= m; d[0, j] = j++)
            {
            }

            // Begin the loop to compute the distance between the strings
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    // Compute the cost of the operation
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;

                    // Update the distance array with the minimum of the three possible operations
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),d[i - 1, j - 1] + cost);
                }
            }
            // Return the distance between the strings
            return d[n, m];
        }
    }
}
