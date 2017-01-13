using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EntryPoint{
#if WINDOWS || LINUX
    public static class Program{
        static List<Vector2> tempList = new List<Vector2>();
        static ITree<Vector2> insert(ITree<Vector2> t, Vector2 v, bool vert = true){
            if (t.isEmpty){
                return new Node<Vector2>(new Empty<Vector2>(), v, new Empty<Vector2>(), !vert);
            }
            if (t.value == v){
                return t;
            }

            if (vert == true){
                if (v.X < t.value.X){
                    return new Node<Vector2>(insert(t.left, v, !vert), t.value, t.right, !vert);
                }
                else{
                    return new Node<Vector2>(t.left, t.value, insert(t.right, v, !vert), !vert);
                }
            }
            else{
                if (v.Y < t.value.Y){
                    return new Node<Vector2>(insert(t.left, v, !vert), t.value, t.right, !vert);
                }
                else{
                    return new Node<Vector2>(t.left, t.value, insert(t.right, v, !vert), !vert);
                }
            }
        }

        static void searchInOrder(ITree<Vector2> t, double radius, Vector2 currentHouse){
            if (!t.isEmpty){
                double distance = Math.Sqrt((Math.Pow((currentHouse.X - t.value.X), 2) + Math.Pow((currentHouse.Y - t.value.Y), 2)));

                if (t.evenForY){ //if true, indexed based on y-axis
                    if (Math.Abs(currentHouse.Y - t.value.Y) < radius){
                        if (distance <= radius){
                            tempList.Add(t.value);
                        }
                        searchInOrder(t.left, radius, currentHouse);
                        searchInOrder(t.right, radius, currentHouse);
                    }
                    else if (t.value.Y > (currentHouse.Y + radius)){
                        searchInOrder(t.left, radius, currentHouse);
                    }
                    else if (t.value.Y < (currentHouse.Y - radius)){
                        searchInOrder(t.right, radius, currentHouse);
                    }
                }
                else{ //indexed based on x-axis
                    if (Math.Abs(currentHouse.X - t.value.X) < radius){
                        if (distance <= radius){
                            tempList.Add(t.value);
                        }
                        searchInOrder(t.left, radius, currentHouse);
                        searchInOrder(t.right, radius, currentHouse);
                    }
                    else if (t.value.X > (currentHouse.X + radius)){
                        searchInOrder(t.left, radius, currentHouse);
                    }
                    else if (t.value.X < (currentHouse.X - radius)){
                        searchInOrder(t.right, radius, currentHouse);
                    }
                }
                //searchInOrder(t.left, radius, currentHouse);
                //if (distance <= radius) {
                //    tempList.Add(t.value);
                //}
                //searchInOrder(t.right, radius, currentHouse);
            }
        }

        [STAThread]
        static void Main(){
            var fullscreen = false;
            read_input:
            switch (Microsoft.VisualBasic.Interaction.InputBox("Which assignment shall run next? (1, 2, 3, 4, or q for quit)", "Choose assignment", VirtualCity.GetInitialValue())){
                case "1":
                    using (var game = VirtualCity.RunAssignment1(SortSpecialBuildingsByDistance, fullscreen))
                        game.Run();
                    break;
                case "2":
                    using (var game = VirtualCity.RunAssignment2(FindSpecialBuildingsWithinDistanceFromHouse, fullscreen))
                        game.Run();
                    break;
                case "3":
                    using (var game = VirtualCity.RunAssignment3(FindRoute, fullscreen))
                        game.Run();
                    break;
                case "4":
                    using (var game = VirtualCity.RunAssignment4(FindRoutesToAll, fullscreen))
                        game.Run();
                    break;
                case "q":
                    return;
            }
            goto read_input;
        }

        private static IEnumerable<Vector2> SortSpecialBuildingsByDistance(Vector2 house, IEnumerable<Vector2> specialBuildings){
            //list off all special buildings
            List<Vector2> allBuildings = specialBuildings.ToList();

            //add distance to house from allBuildings together with location in a tuple inside a list
            Tuple<Vector2, double>[] BuildingsWithDistance = new Tuple<Vector2, double>[50];
            int cnt = 0;
            foreach (Vector2 singleBuilding in allBuildings){
                double distance = Math.Sqrt((Math.Pow((house.X - singleBuilding.X), 2) + Math.Pow((house.Y - singleBuilding.Y), 2)));
                BuildingsWithDistance[cnt] = new Tuple<Vector2, double>(singleBuilding, distance);
                cnt++;
            }

            //merge sort the objects on distance
            mergeSort(BuildingsWithDistance, 0, BuildingsWithDistance.Length - 1);

            List<Vector2> finalList = new List<Vector2>();
            foreach (Tuple<Vector2, double> x in BuildingsWithDistance){
                finalList.Add(x.Item1);
            }
            return finalList;
            //return specialBuildings.OrderBy(v => Vector2.Distance(v, house));
        }

        private static IEnumerable<IEnumerable<Vector2>> FindSpecialBuildingsWithinDistanceFromHouse(
            IEnumerable<Vector2> specialBuildings,
            IEnumerable<Tuple<Vector2, float>> housesAndDistances){

            //create 2d tree
            var t = new Empty<Vector2>() as ITree<Vector2>;
            foreach (Vector2 specialBuilding in specialBuildings){
                t = insert(t, specialBuilding);
            }

            //find buildings in radius and add them in a list
            List<List<Vector2>> combinedList = new List<List<Vector2>>();
            foreach (Tuple<Vector2, float> DistanceAndHouse in housesAndDistances){
                tempList.Clear();
                double radius = DistanceAndHouse.Item2;
                searchInOrder(t, radius, DistanceAndHouse.Item1);

                List<Vector2> varList = new List<Vector2>();
                foreach (Vector2 x in tempList){
                    varList.Add(x);
                }
                combinedList.Add(varList);
            }

            return combinedList;
            //return
            //    from h in housesAndDistances
            //    select
            //    from s in specialBuildings
            //    where Vector2.Distance(h.Item1, s) <= h.Item2
            //    select s;
        }

        private static IEnumerable<Tuple<Vector2, Vector2>> FindRoute(Vector2 startingBuilding,
            Vector2 destinationBuilding, IEnumerable<Tuple<Vector2, Vector2>> roads){

            List<Tuple<Vector2, Vector2>> finalPath = new List<Tuple<Vector2, Vector2>>();
            //int roadsNumber = roads.Count() + 1;
            Vector2[] nodes = new Vector2[2500];
            bool[,] matrix = new bool[2500, 2500];
            int cnt = 0;
            //set everything in matrix to false
            for (int i = 0; i < nodes.Length; i++){
                for (int j = 0; j < nodes.Length; j++){
                    matrix[i, j] = false;
                }
            }

            // generate adjacency matrix
            foreach(Tuple<Vector2, Vector2> road in roads){
                if (!nodes.Contains(road.Item1) && !nodes.Contains(road.Item2)){
                    nodes[cnt] = road.Item1;
                    matrix[cnt, cnt] = true;
                    cnt++;
                    nodes[cnt] = road.Item2;
                    matrix[cnt, cnt] = true;
                    matrix[cnt - 1, cnt] = true;
                    matrix[cnt, cnt - 1] = true;
                    cnt++;
                }
                else if (nodes.Contains(road.Item1) && !nodes.Contains(road.Item2)){
                    nodes[cnt] = road.Item2;
                    matrix[cnt, cnt] = true;
                    int index = Array.IndexOf(nodes, road.Item1);
                    matrix[index, cnt] = true;
                    matrix[cnt, index] = true;
                    cnt++;
                }
                else if (!nodes.Contains(road.Item1) && nodes.Contains(road.Item2)){
                    nodes[cnt] = road.Item1;
                    matrix[cnt, cnt] = true;
                    int index = Array.IndexOf(nodes, road.Item2);
                    matrix[index, cnt] = true;
                    matrix[cnt, index] = true;
                    cnt++;
                }
                else if (nodes.Contains(road.Item1) && nodes.Contains(road.Item2)){
                    int index1 = Array.IndexOf(nodes, road.Item1);
                    int index2 = Array.IndexOf(nodes, road.Item2);
                    matrix[index1, index2] = true;
                    matrix[index2, index1] = true;                   
                }
            }

            int startIndex = Array.FindIndex(nodes, v => v.Equals(startingBuilding)); // index of startinghouse
            int endIndex = Array.FindIndex(nodes, v => v.Equals(destinationBuilding)); // index of destination
            int nodeCount = cnt; //amount of nodes
            List<int> nodesIndex = new List<int>(); // list with indexes
            List<float> distances = new List<float>(); // list with infite distances linked by index
            for (int i = 0; i < nodeCount; i++){
                nodesIndex.Add(i);
                distances.Add(float.MaxValue);
            }

            distances[startIndex] = 0;
            int[] prev = new int[nodeCount]; // array with indexes of the path

            while (nodesIndex.Count > 0){
                // find index of node with the smallest distance
                int index = -1;
                float shortest = float.MaxValue;
                for (int i = 0; i < distances.Count(); i++)
                {
                    if (nodesIndex.Contains(i))
                    {
                        if (distances[i] < shortest)
                        {
                            shortest = distances[i];
                            index = i;
                        }
                    }
                }
                int closestIndex = index;

                for (int i = 0; i < nodeCount; i++){
                    if (matrix[i, closestIndex]){ //look in matrix if nodes are connected
                        float tempDist = distances[closestIndex] + Vector2.Distance(nodes[closestIndex], nodes[i]);

                        if (tempDist < distances[i]){
                            distances[i] = tempDist;
                            prev[i] = closestIndex;
                        }
                    }
                }
                nodesIndex.Remove(closestIndex);
            }

            int tempIndex = endIndex;

            while (tempIndex != startIndex){
                int previous = prev[tempIndex];
                finalPath.Add(new Tuple<Vector2, Vector2>(nodes[tempIndex], nodes[previous]));
                tempIndex = prev[tempIndex];
            }

            return finalPath;

            //var startingRoad = roads.Where(x => x.Item1.Equals(startingBuilding)).First();
            //List<Tuple<Vector2, Vector2>> fakeBestPath = new List<Tuple<Vector2, Vector2>>() { startingRoad };
            //var prevRoad = startingRoad;
            //for (int i = 0; i < 30; i++)
            //{
            //    prevRoad = (roads.Where(x => x.Item1.Equals(prevRoad.Item2)).OrderBy(x => Vector2.Distance(x.Item2, destinationBuilding)).First());
            //    fakeBestPath.Add(prevRoad);
            //}
            //return fakeBestPath;
        }


        private static IEnumerable<IEnumerable<Tuple<Vector2, Vector2>>> FindRoutesToAll(Vector2 startingBuilding,
          IEnumerable<Vector2> destinationBuildings, IEnumerable<Tuple<Vector2, Vector2>> roads){
            List<List<Tuple<Vector2, Vector2>>> result = new List<List<Tuple<Vector2, Vector2>>>();
            foreach (var d in destinationBuildings){
                var startingRoad = roads.Where(x => x.Item1.Equals(startingBuilding)).First();
                List<Tuple<Vector2, Vector2>> fakeBestPath = new List<Tuple<Vector2, Vector2>>() { startingRoad };
                var prevRoad = startingRoad;
                for (int i = 0; i < 30; i++){
                    prevRoad = (roads.Where(x => x.Item1.Equals(prevRoad.Item2)).OrderBy(x => Vector2.Distance(x.Item2, d)).First());
                    fakeBestPath.Add(prevRoad);
                }
                result.Add(fakeBestPath);
            }
            return result;
        }

        //start merge sort
        private static void mergeSort(Tuple<Vector2, double>[] BuildingsWithDistance, int left, int right){
            int mid;
            if (right > left){
                mid = (right + left) / 2;
                mergeSort(BuildingsWithDistance, left, mid);
                mergeSort(BuildingsWithDistance, (mid + 1), right);
                merging(BuildingsWithDistance, left, (mid + 1), right);
            }
        }

        private static void merging(Tuple<Vector2, double>[] BuildingsWithDistance, int left, int mid, int right){
            Tuple<Vector2, double>[] tempList = new Tuple<Vector2, double>[50];

            int leftRange = (mid - 1);
            int tmp_pos = left;
            int num_elements = (right - left + 1);

            while (left <= leftRange && (mid <= right)){
                if (BuildingsWithDistance[left] != null || BuildingsWithDistance[mid] != null){
                    if (BuildingsWithDistance[left].Item2 <= BuildingsWithDistance[mid].Item2){
                        tempList[tmp_pos++] = BuildingsWithDistance[left++];
                    }
                    else{
                        tempList[tmp_pos++] = BuildingsWithDistance[mid++];
                    }
                }
            }

            while (left <= leftRange){
                tempList[tmp_pos++] = BuildingsWithDistance[left++];
            }
            while (mid <= right){
                tempList[tmp_pos++] = BuildingsWithDistance[mid++];
            }

            for (int i = 0; i < num_elements; i++){
                BuildingsWithDistance[right] = tempList[right];
                right--;
            }
        }
        //end merge sort

    }
#endif
}
