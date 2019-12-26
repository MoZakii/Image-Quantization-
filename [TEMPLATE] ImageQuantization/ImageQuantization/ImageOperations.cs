using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Globalization;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }


    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {

            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }
            return Buffer;
        }
        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        /// 
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }

        //OUR CODE IS HERE LOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOOL

        public static class Globals
        {
            public static double sum = 0;
            public static int distinct = 0;
            public static long Time;
            public static int[,] Image;
            public static RGBPixel[] Representative;
        }


        public static RGBPixel[] FillGraph(RGBPixel[,] ImageMatrix)
        {
            int index = 0;
            int size = 0;
            string hex;
            
            Dictionary<string,int> L = new Dictionary<string,int>();
            Globals.Image  = new int[GetHeight(ImageMatrix), GetWidth(ImageMatrix)];
            for (int j = 0; j < GetHeight(ImageMatrix); j++)
            {
                for (int l = 0; l < GetWidth(ImageMatrix); l++)
                {            
                    if (size != L.Count)
                    {
                        index++;
                    }
                    size = L.Count;

                    hex = ImageMatrix[j,l].red.ToString("X2") + ImageMatrix[j, l].green.ToString("X2") + ImageMatrix[j, l].blue.ToString("X2");

                    if (!L.ContainsKey(hex))
                    {
                        //L.Add(ImageMatrix[j, l].ToString(), index);
                        L[hex] = index;
                    }
                    Globals.Image[j,l] = L[hex];
                }
            }

            RGBPixel[] Nodes = new RGBPixel[L.Count];
            RGBPixel Color = new RGBPixel();

            foreach (var n in L)
            {
                
                Color.red = (byte)int.Parse(n.Key.Substring(0, 2), NumberStyles.AllowHexSpecifier);
                Color.green = (byte)int.Parse(n.Key.Substring(2, 2), NumberStyles.AllowHexSpecifier);
                Color.blue = (byte)int.Parse(n.Key.Substring(4, 2), NumberStyles.AllowHexSpecifier);
                
                Nodes[n.Value] = Color;              
            }

            Globals.distinct = L.Count;
            L.Clear();
            
            return Nodes;
        }

        //Get the node with minimum distance and not visited before to relax the neighbours from it.

        public class Edge
        {

            public int source;

            public int destination;

            public double weight;

            public Edge(int source, int destination, double weight)
            {

                this.source = source;

                this.destination = destination;

                this.weight = weight;

            }
        }

        public class HeapNode
        {

            public int vertex;
            public double key;

        }



        public class Graph
        {

            public int vertices;

            public LinkedList<Edge>[] adjacencylist;

            public Graph(int vertices)
            {

                this.vertices = vertices;

                adjacencylist = new LinkedList<Edge>[vertices];

                //initialize adjacency lists for all the vertices

                for (int i = 0; i < vertices; i++)
                {

                    adjacencylist[i] = new LinkedList<Edge>();

                }

            }

            public virtual void primMST(int[] parent, double[] min_weights, int vertices, RGBPixel[] Nodes)
            {


                

                bool[] inHeap = new bool[vertices];
                double[] key = new double[vertices];

                //create heapNode for all the vertices

                HeapNode[] heapNodes = new HeapNode[vertices];

                for (int i = 0; i < vertices; i++)
                {

                    heapNodes[i] = new HeapNode(); //heapnode contains(vertex,key)
                    heapNodes[i].vertex = i;
                    heapNodes[i].key = int.MaxValue;
                    parent[i] = int.MaxValue;
                    min_weights[i] = double.MaxValue;
                    inHeap[i] = true;
                    key[i] = int.MaxValue;

                }
                //decrease the key for the first index

                heapNodes[0].key = 0;
                //add all the vertices to the MinHeap

                MinHeap minHeap = new MinHeap(vertices);

                //add all the vertices to priority queue

                for (int i = 0; i < vertices; i++)
                {
                    minHeap.insert(heapNodes[i]);

                }

                //while minHeap is not empty
                parent[0] = -1;
                min_weights[0] = 0;
                while (!minHeap.Empty)
                {

                    //extract the min

                    HeapNode extractedNode = minHeap.extractMin();

                    //extracted vertex

                    int extractedVertex = extractedNode.vertex;

                    inHeap[extractedVertex] = false;  //false because it has been extracted.

                    //iterate through all the adjacent vertices

                    //LinkedList<Edge> list = adjacencylist[extractedVertex];


                    double weght;


                    for (int l = 0; l < vertices; l++)
                    {

                        weght = ((Nodes[extractedVertex].red - Nodes[l].red) * (Nodes[extractedVertex].red - Nodes[l].red)) + ((Nodes[extractedVertex].blue - Nodes[l].blue )* (Nodes[extractedVertex].blue - Nodes[l].blue)) + ((Nodes[extractedVertex].green - Nodes[l].green) * (Nodes[extractedVertex].green - Nodes[l].green));

                        //graph.addEdge(j, l, weght);
                        if (inHeap[l])
                        {

                            int destination = l;

                            double newKey = weght;

                            //check if updated key < existing key, if yes, update if

                            if (key[destination] > newKey)
                            {

                                decreaseKey(minHeap, newKey, destination);

                                //update the parent node for destination

                                parent[destination] = extractedVertex;

                                min_weights[destination] = Math.Sqrt(newKey);

                                key[destination] = newKey; //destination vertex carries the weight between it and its source u in old code

                            }
                        }
                    }
                    //Edge edge = i;

                    //only if edge destination is present in heap

                }

                Globals.sum = 0;
                for (int j = 0; j < vertices; j++)      //calculates the sum of the MST
                {
                    Globals.sum += min_weights[j];
                }
                Globals.sum = Math.Round(Globals.sum, 2);
            }

            public virtual void decreaseKey(MinHeap minHeap, double newKey, int vertex)
            {

                //get the index which key's needs a decrease;

                int index = minHeap.indexes[vertex];



                //get the node and update its value

                HeapNode node = minHeap.mH[index];

                node.key = newKey;

                minHeap.bubbleUp(index);

            }



        }

        public class MinHeap
        {

            internal int capacity;

            internal int currentSize;

            internal HeapNode[] mH;

            internal int[] indexes; //will be used to decrease the key

            public MinHeap(int capacity)
            {

                this.capacity = capacity;

                mH = new HeapNode[capacity + 1];

                indexes = new int[capacity];

                mH[0] = new HeapNode();

                mH[0].key = int.MinValue;

                mH[0].vertex = -1;

                currentSize = 0;

            }



            public virtual void insert(HeapNode x)
            {

                currentSize++;

                int idx = currentSize;

                mH[idx] = x;

                indexes[x.vertex] = idx;

                bubbleUp(idx);

            }

            public virtual void bubbleUp(int pos)
            {

                int parentIdx = pos / 2;

                int currentIdx = pos;

                while (currentIdx > 0 && mH[parentIdx].key > mH[currentIdx].key)
                {

                    HeapNode currentNode = mH[currentIdx];

                    HeapNode parentNode = mH[parentIdx];



                    //swap the positions

                    indexes[currentNode.vertex] = parentIdx;

                    indexes[parentNode.vertex] = currentIdx;

                    swap(currentIdx, parentIdx);

                    currentIdx = parentIdx;

                    parentIdx = parentIdx / 2;

                }

            }

            public virtual HeapNode extractMin()
            {

                HeapNode min = mH[1];

                HeapNode lastNode = mH[currentSize];

                //            update the indexes[] and move the last node to the top

                indexes[lastNode.vertex] = 1;

                mH[1] = lastNode;

                mH[currentSize] = null;

                sinkDown(1);

                currentSize--;

                return min;

            }

            public virtual void sinkDown(int k)
            {

                int smallest = k;

                int leftChildIdx = 2 * k;

                int rightChildIdx = 2 * k + 1;

                if (leftChildIdx < heapSize() && mH[smallest].key > mH[leftChildIdx].key)
                {

                    smallest = leftChildIdx;

                }

                if (rightChildIdx < heapSize() && mH[smallest].key > mH[rightChildIdx].key)
                {

                    smallest = rightChildIdx;

                }

                if (smallest != k)
                {



                    HeapNode smallestNode = mH[smallest];

                    HeapNode kNode = mH[k];



                    //swap the positions

                    indexes[smallestNode.vertex] = k;

                    indexes[kNode.vertex] = smallest;

                    swap(k, smallest);

                    sinkDown(smallest);

                }

            }

            public virtual void swap(int a, int b)
            {

                HeapNode temp = mH[a];

                mH[a] = mH[b];

                mH[b] = temp;

            }

            public virtual bool Empty
            {
                get
                {

                    return currentSize == 0;

                }
            }

            public virtual int heapSize()
            {

                return currentSize;

            }

        }



        public static List<KeyValuePair<double, int>> MST(int vertices, RGBPixel[] Nodes)
        {


            int[] parent = new int[vertices];
            double[] min_weights = new double[vertices];

            Graph graph = new Graph(vertices);

            graph.primMST(parent, min_weights, vertices, Nodes);

            List<KeyValuePair<double, int>> Edges = new List<KeyValuePair<double, int>>();
            for (int l = 0; l < vertices; l++)     //creates array of list stores node with(neighbour, weight). 
            {
                Edges.Add(new KeyValuePair<double, int>(min_weights[l], parent[l]));
            }
            return Edges;

        }

        public static List<int>[] Clustering(List<KeyValuePair<double, int>> NewEdges, int V, int numOfCluster)
        {
            int[] visited = new int[NewEdges.Count];                                                                            //        O ( 1 )
            List<int> temp = new List<int>();                                                                                   //        O ( 1 )
            List<int>[] Colors = new List<int>[numOfCluster];                                                                   //        O ( 1 )
            int index = -1, index2 = 0, j = 0, iterator;                                                                        //        O ( 1 )
            double max = -1;                                                                                                    //        O ( 1 )

            for (int i = 0; i < numOfCluster - 1; i++)                                                                          //      O ( K * D )
            {
                max = -1;                                                                                                       //        O ( 1 )

                for (int l = 1; l < NewEdges.Count; l++)                                                                        //        O ( D )
                {
                    if (NewEdges[l].Key > max)                                                                                  //        O ( 1 )
                    {
                        max = NewEdges[l].Key;                                                                                  //        O ( 1 )
                        index = l;                                                                                              //        O ( 1 )
                    }
                }
                NewEdges.RemoveAt(index);                                                                                       //        O ( 1 )
                NewEdges.Insert(index, new KeyValuePair<double, int>(0, 0));                                                    //        O ( 1 )
            }

            for (int i = 0; i < NewEdges.Count; i++)                                                                            //        O ( D )
            {
                visited[i] = -1;                                                                                                //        O ( 1 )
            }
            for (int i = 0; i < numOfCluster; i++)                                                                             //        O ( K )
            {
                Colors[i] = new List<int>();                                                                                    //        O ( 1 )
            }


            for (int i = 0; i < NewEdges.Count; i++)                                                                                                      //      Exact ( D )
            {
                iterator = i;                                                                                                                             //        O ( 1 )

                while (NewEdges[iterator].Key != 0 && visited[iterator] == -1)  //Until ending node is reached && if not visited add to vector            //      Upper ( D )
                {
                    temp.Add(iterator);                                                                                                                   //        O ( 1 )
                    iterator = NewEdges[iterator].Value;                                                                                                  //        O ( 1 )
                }

                if (NewEdges[iterator].Key != 0 && visited[iterator] != -1)  //else add last node's index and break.                                      //        O ( 1 )
                    index2 = visited[iterator];                                                                                                           //        O ( 1 )

                if (NewEdges[iterator].Key == 0 && visited[iterator] == -1) //if it reached an unknown end.                                               //        O ( 1 )
                {
                    index2 = j; //For later use in next loop.                                                                                             //        O ( 1 )
                    visited[iterator] = j;                                                                                                                //        O ( 1 )
                    temp.Add(iterator);                                                                                                                   //        O ( 1 )
                    j++;                                                                                                                                  //        O ( 1 )
                }
                else if (NewEdges[iterator].Key == 0 && visited[iterator] != -1)  //if it reached a known end.                                            //        O ( 1 )
                    index2 = visited[iterator];                                                                                                           //        O ( 1 )
                for (int l = 0; l < temp.Count; l++) //Adds vector into the list.                                                                         //      Upper ( D )
                {
                    Colors[index2].Add(temp[l]);                                                                                                          //        O ( 1 )
                    visited[temp[l]] = index2;                                                                                                            //        O ( 1 )
                }
                temp.Clear();                                                                                                                             //      Upper ( D )
            }
            NewEdges.Clear();                                                                                                                             //      Exact ( D )
            return Colors;
        }

        public static int[] Pallete(List<int>[] Colors, int k, RGBPixel[] Nodes)
        {
            
            int[] indices = new int[Globals.distinct];
            Globals.Representative = new RGBPixel[k];      
            double R, G, B;
            for (int i = 0; i < k; i++) // Exact O(D)
            {
                R = 0; G = 0; B = 0;

                foreach (var n in Colors[i]) //Upper O(D)
                {
                    R += Nodes[n].red;
                    G += Nodes[n].green;
                    B += Nodes[n].blue;
                }

                R /= Colors[i].Count;
                G /= Colors[i].Count;
                B /= Colors[i].Count;

                Globals.Representative[i].red = (byte)R;
                Globals.Representative[i].green = (byte)G;
                Globals.Representative[i].blue = (byte)B;

            }
            for (int i = 0; i < k; i++)
            {
                foreach (var n in Colors[i])
                {
                    indices[n] = i;
                }
            }
            return indices;
            
        }

        public static RGBPixel[,] ImageQuantization(RGBPixel[,] ImageMatrix, int[] indices , RGBPixel[] Nodes)
        {
            for (int j = 0; j < ImageOperations.GetHeight(ImageMatrix); j++)
            {
                for (int l = 0; l < ImageOperations.GetWidth(ImageMatrix); l++)
                {
                    ImageMatrix[j, l] = Globals.Representative[indices[Globals.Image[j,l]]];
                }
            }
            return ImageMatrix;
        }

    //    public static int NumOfClusters(List<KeyValuePair<double, int>> NewEdges)
    //    {
    //        int counter = 1,index=0,parent;
    //        double stdev = GetStandardDeviation(NewEdges,counter),stdev2=0,difference = stdev,min,weight,weight2=0,oldDifference=0;
    //        while (difference > 0.0001)
    //        {
    //            min = 1.79769313486232E+307;
    //            for (int l = 1; l < NewEdges.Count; l++)
    //            {
    //                weight = NewEdges[l].Key;
    //                parent = NewEdges[l].Value;
    //                NewEdges.RemoveAt(l);
    //                NewEdges.Insert(l, new KeyValuePair<double, int>(0, 0));
    //                counter++;
    //                stdev2 = GetStandardDeviation(NewEdges, counter);
    //                counter--;
    //                NewEdges.RemoveAt(l);
    //                NewEdges.Insert(l, new KeyValuePair<double, int>(weight, parent));
    //                if (stdev2 < min)
    //                {
    //                    min = stdev2;
    //                    index = l;
    //                }
    //            }
    //            NewEdges.RemoveAt(index);
    //            NewEdges.Insert(index, new KeyValuePair<double, int>(0, 0));
    //            weight2 = stdev - min;
    //            oldDifference = difference - oldDifference;
    //            difference = oldDifference - weight2;
    //            oldDifference = weight2;
    //            stdev = min;
    //            counter++;
    //        }
    //        return counter-2;
    //    }
    //    // Return the standard deviation of an array of Doubles.

    //    public static double GetStandardDeviation(List<KeyValuePair<double, int>> values,int counter)
    //    {
    //        double standardDeviation = 0,sum = 0,avg,temp=0;
    //        foreach (var n in values)
    //        {
    //            sum += n.Key;
    //        }
    //        int count = values.Count;
    //        count -= counter;
    //        if (count > 1)
    //        {
    //            avg = sum / count;
    //            foreach (var n in values)
    //            {
    //                if(n.Key != 0)
    //                    temp += (n.Key - avg) * (n.Key - avg);
    //            }
    //            standardDeviation = Math.Sqrt(temp / count);
    //        }
    //        return standardDeviation;
    //    }
    }

}

