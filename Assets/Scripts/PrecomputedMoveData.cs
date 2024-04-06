namespace Chess
{
    public static class PrecomputedMoveData
    {
        // First 4 are orthogonal, last 4 are diagonals (N, S, W, E, NW, SE, NE, SW)
        public static readonly int[] directionOffsets = { 8, -8, -1, 1, 7, -7, 9, -9 };
        public static readonly int[][] numSquaresToEdge = new int[64][];
        public static int[] knightOffsets = { -17, -15, -10, -6, 6, 10, 15, 17 };
        public static int[] kingOffsets = { -9, -8, -7, -1, 1, 7, 8, 9 };

        static PrecomputedMoveData()
        {
            for (int file = 0; file < 8; file++)
            {
                for (int rank = 0; rank < 8; rank++)
                {
                    int numNorth = 7 - rank;
                    int numSouth = rank;
                    int numWest = file;
                    int numEast = 7 - file;

                    int squareIndex = rank * 8 + file;

                    numSquaresToEdge[squareIndex] = new int[]
                    {
                    numNorth,
                    numSouth,
                    numWest,
                    numEast,
                    System.Math.Min(numNorth, numWest),
                    System.Math.Min(numSouth, numEast),
                    System.Math.Min(numNorth, numEast),
                    System.Math.Min(numSouth, numWest)
                    };
                }
            }
        }
    }
}