using System;
using System.Windows.Forms;
using System.Numerics;
using System.Drawing;
using System.Collections.Generic;

public class ProgramController
{
    //World defining variables: Set in the valueDefinion() function
    //Frequency increases the density of regions
    (int width, int height) worldDimensions = (512, 512);

    //An array of each ore, including the percentage composition and the frequency of the voronoi noise
    //The Block object contains a color to draw and the colors that it can grow in. In actual implementation this shall
    //be more complex and will instead of referencing colors to grow in, reference block types themselves to remove dependencies. 
    (Block ore, double percentage, double frequency)[] oreComposition =
    { (new Block(Color.Black, new List<Color>()), 100, 0.01), //Base block
    (new Block(Color.FromArgb(150, 150, 150), new List<Color>{Color.Black}), 0.1, 0.06),    
    (new Block(Color.Crimson, new List<Color>{Color.Black}), 0.1, 0.4),
    (new Block(Color.DarkGreen, new List<Color>{Color.FromArgb(150, 150, 150)}), 0.05, 0.6)};

    //Percentage determinator: worldDimensions 
    

    (int width, int height) screenDimensions = (900, 900);



    public void initialiseProgram()
    {
        WorldContext worldContext = new WorldContext(worldDimensions.width, worldDimensions.height, oreComposition);


        worldContext.generateWorld(oreComposition);

        CustomWindow window = new CustomWindow(worldContext, screenDimensions, false);


        window.ShowDialog();
    }

}

public class ProgramInitialiser
{
    public static void Main(string[] args)
    {
        ProgramController pc = new ProgramController();
        pc.initialiseProgram();
    }
}

public class CustomWindow : Form
{
    WorldContext worldContext;

    (int width, int height) windowSize;

    bool inGreyscale = false;

    public CustomWindow(WorldContext wc, (int width, int height) windowSize, bool isInGreyscale)
    {
        Size = new System.Drawing.Size(windowSize.width, windowSize.height);
        this.windowSize = windowSize;
        worldContext = wc;
        inGreyscale = isInGreyscale;

    }

    protected override void OnPaint(PaintEventArgs e)
    {
        Graphics graphics = e.Graphics;


        int lowResolutionLengthX = worldContext.getBlockArray().GetLength(0);
        int lowResolutionLengthY = worldContext.getBlockArray().GetLength(1);

        Bitmap lowResolution = new Bitmap(lowResolutionLengthX, lowResolutionLengthY);

        for (int x = 0; x < lowResolutionLengthX; x++)
        {
            for (int y = 0; y < lowResolutionLengthY; y++)
            {
                
                
                    lowResolution.SetPixel(x, y, worldContext.getBlockArray()[x, y].color);
                
            }
        }

        graphics.DrawImage(resizeBitmap(lowResolution, windowSize), 0, 0);

        /* //Draw the numbers of each vein
        for (int x = 0; x < lowResolutionLengthX; x++)
        {
            for (int y = 0; y < lowResolutionLengthY; y++)
            {
                int screenspaceLocationX = (int)(((double)x / (double)worldContext.getWorldArray().GetLength(0)) * 900);
                int screenspaceLocationY = (int)(((double)y / (double)worldContext.getWorldArray().GetLength(1)) * 900);
                
                SolidBrush b = new SolidBrush(Color.Black);
                PointF point = new PointF(screenspaceLocationX, screenspaceLocationY);
                graphics.DrawString(worldContext.getWorldArray()[x, y].ToString(), new Font("Ariel", 16), b, point);

            }
        }
        */
            }

    public Bitmap resizeBitmap(Bitmap source, (int x, int y) outputSize)
    {
        Bitmap result = new Bitmap(outputSize.x, outputSize.y);

        using (Graphics g = Graphics.FromImage(result))
        {
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            g.DrawImage(source,0, 0, outputSize.x, outputSize.y);
        }
        return result;
     }
}

public class WorldContext
{
    double[][,] worldArray;

    Block[,] blockArray;

    int seed;

    public WorldContext(int worldX, int worldY, (Block block, double composition, double frequency)[] worldComposition)
    {
        worldArray = new double[worldComposition.Length][,];
        for (int i = 0; i < worldArray.Length; i++)
        {
            worldArray[i] = new double[worldX, worldY];
        }
        blockArray = new Block[worldX, worldY];
    }

    public void generateBlockArray(double[][,] worldArray, (Block block, double composition, double frequency)[] oreComposition)
    {
        //two seperate lists rather than a single tuple list so that the .Find() function can be used
        for (int i = 0; i < worldArray.Length; i++)
        {
            List<double> definedVeins = new List<double>();
            List<Block> definedVeinsBlock = new List<Block>();
            int numOfVeins = 0;
            for (int x = 0; x < worldArray[i].GetLength(0); x++)
            {

                for (int y = 0; y < worldArray[i].GetLength(1); y++)
                {
                    bool canGrow = false;
                    if (blockArray[x, y] != null)
                    {
                        if (oreComposition[i].block.canGrowIn.Contains(blockArray[x, y].color))
                        {
                            canGrow = true;
                        }
                    }
                    else
                    {
                        canGrow = true;
                     }
                    if (canGrow)
                    {
                        if (!definedVeins.Contains(worldArray[i][x, y]))
                        {
                            //Generates a seeded random that will always return the same value for the same value of the world array.
                            //ie a random number dependent on the closest point
                            Random random = new Random();
                            double outputPercentage = random.NextDouble() * 100;


                            if (outputPercentage < oreComposition[i].composition)
                            {

                                blockArray[x, y] = oreComposition[i].block;

                                definedVeins.Add(worldArray[i][x, y]);
                                definedVeinsBlock.Add(oreComposition[i].block);
                                numOfVeins++;
                            }


                        }
                        else
                        {
                            //Technically the index will just be the worldArray[x,y] - 1 value, as it is going sequentially,
                            //But using that is not modification proof

                            int indexOfVein = definedVeins.IndexOf(worldArray[i][x, y]);
                            blockArray[x, y] = definedVeinsBlock[indexOfVein];
                        }
                    }
                }

            }
            
        }
    }
    
    public void regrowSeed()
    {
        Random r = new Random();
        seed = r.Next();
    }

    public double[][,] getWorldArray()
    {
        return worldArray;
    }

    public Block[,] getBlockArray()
    {
        return blockArray;
    }


    public void generateWorld((Block ore, double composition, double frequency)[] oreComposition)
    {
        VoronoiNoise vn = new VoronoiNoise();

        for (int i = 0; i < worldArray.Length; i++)
        {
            worldArray[i] = vn.generateNoise((worldArray[i].GetLength(0), worldArray[i].GetLength(0)), oreComposition[i].frequency);
        }

        regrowSeed();
        generateBlockArray(worldArray, oreComposition);
    }
}

public class Block
{
    public Color color;
    public List<Color> canGrowIn;
    public Block(Color color, List<Color> canGrowIn)
    {
        this.color = color;
        this.canGrowIn = canGrowIn;
    } 
}

public class VoronoiNoise
{
    //Noise generated by positioning a point inside of a grid, then going through each pixel and determining which point is closest

    public double[,] generateNoise((int width, int height)worldDimensions, double frequency)
    {
        double[,] noiseOutput = new double[worldDimensions.width, worldDimensions.height];

        Vector2[,] unitSquarePoints = distributeRandomPoints(worldDimensions, frequency);

        for (int x = 0; x < noiseOutput.GetLength(0); x++)
        {
            for (int y = 0; y < noiseOutput.GetLength(1); y++)
            {
                noiseOutput[x, y] = noiseAlgorithm(x * frequency, y * frequency, unitSquarePoints);
            }
        }

        return noiseOutput;
    }


    private Vector2[,] distributeRandomPoints((int width, int height)worldDimensions, double frequency)
    {
        //Determines the number of unit squares. 1/Frequency represents the number of worldArray coordinates per unit
        //So multiplying the length of the worldArray by the frequency, gives the number of units per array. The +1 ensures no index overflows
        Vector2[,] unitSquareVectors = new Vector2[(int)Math.Ceiling(worldDimensions.width * frequency) + 1, (int)Math.Ceiling(worldDimensions.height * frequency) + 1];

        for (int x = 0; x < unitSquareVectors.GetLength(0); x++)
        {
            for (int y = 0; y < unitSquareVectors.GetLength(1); y++)
            {
                Random r = new Random();

                //Assigns a random point within the unit square
                unitSquareVectors[x, y] = Vector2.Create((float)r.NextDouble() + x, (float)r.NextDouble() + y);
            }
        }

        Console.WriteLine("Frequncy: " + frequency);
        Console.WriteLine("Length of unit square vectors" + unitSquareVectors.GetLength(0) + ", " + unitSquareVectors.GetLength(1));

        return unitSquareVectors;
    }

    private double noiseAlgorithm(double x, double y, Vector2[,] unitSquares)
    {
        double closestSquare = 0;
        int xInt = (int)Math.Floor(x);
        int yInt = (int)Math.Floor(y);

        double shortestDistance = 100;

        for (int xSquare = xInt - 2; xSquare < xInt + 2; xSquare++)
        {
            for (int ySquare = yInt - 2; ySquare < yInt + 2; ySquare++)
            {
                if (xSquare >= 0 && xSquare < unitSquares.GetLength(0) && ySquare >= 0 && ySquare < unitSquares.GetLength(1))
                {
                    double xDifference = x - unitSquares[xSquare, ySquare].X;
                    double yDifference = y - unitSquares[xSquare, ySquare].Y;

                    //Pythag: d = Sqrt(x^2 + y^2)
                    double distance = Math.Pow(Math.Pow(xDifference, 2) + Math.Pow(yDifference, 2), 0.5);


                    if (distance < shortestDistance)
                    {
                        closestSquare = xSquare + ySquare * unitSquares.GetLength(0);
                        shortestDistance = distance;
                    }
                }
            }
        }

        //return shortestDistance/2;    //Outputs a greyscale texture
        return closestSquare;

    }
}