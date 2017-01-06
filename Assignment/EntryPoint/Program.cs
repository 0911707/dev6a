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
        if (!t.isEmpty) {
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
            double distance = Math.Sqrt( (Math.Pow((house.X - singleBuilding.X), 2) + Math.Pow((house.Y - singleBuilding.Y), 2)) );
            BuildingsWithDistance[cnt] = new Tuple<Vector2, double>(singleBuilding, distance);
            cnt++;
        }

        //merge sort the objects on distance
        mergeSort(BuildingsWithDistance, 0, BuildingsWithDistance.Length - 1);

        List<Vector2> finalList = new List<Vector2>();
        foreach (Tuple<Vector2, double> x in BuildingsWithDistance) {
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
        foreach(Vector2 specialBuilding in specialBuildings){
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
      Vector2 destinationBuilding, IEnumerable<Tuple<Vector2, Vector2>> roads)
    {
      var startingRoad = roads.Where(x => x.Item1.Equals(startingBuilding)).First();
      List<Tuple<Vector2, Vector2>> fakeBestPath = new List<Tuple<Vector2, Vector2>>() { startingRoad };
      var prevRoad = startingRoad;
      for (int i = 0; i < 30; i++)
      {
        prevRoad = (roads.Where(x => x.Item1.Equals(prevRoad.Item2)).OrderBy(x => Vector2.Distance(x.Item2, destinationBuilding)).First());
        fakeBestPath.Add(prevRoad);
      }
      return fakeBestPath;
    }

    private static IEnumerable<IEnumerable<Tuple<Vector2, Vector2>>> FindRoutesToAll(Vector2 startingBuilding, 
      IEnumerable<Vector2> destinationBuildings, IEnumerable<Tuple<Vector2, Vector2>> roads)
    {
      List<List<Tuple<Vector2, Vector2>>> result = new List<List<Tuple<Vector2, Vector2>>>();
      foreach (var d in destinationBuildings)
      {
        var startingRoad = roads.Where(x => x.Item1.Equals(startingBuilding)).First();
        List<Tuple<Vector2, Vector2>> fakeBestPath = new List<Tuple<Vector2, Vector2>>() { startingRoad };
        var prevRoad = startingRoad;
        for (int i = 0; i < 30; i++)
        {
          prevRoad = (roads.Where(x => x.Item1.Equals(prevRoad.Item2)).OrderBy(x => Vector2.Distance(x.Item2, d)).First());
          fakeBestPath.Add(prevRoad);
        }
        result.Add(fakeBestPath);
      }
      return result;
    }
    
    //start merge sort
    private static void mergeSort(Tuple<Vector2, double>[] BuildingsWithDistance, int left, int right)
        {
            int mid;
            if (right > left)
            {
                mid = (right + left) / 2;
                mergeSort(BuildingsWithDistance, left, mid);
                mergeSort(BuildingsWithDistance, (mid + 1), right);
                merging(BuildingsWithDistance, left, (mid + 1), right);
            }
        }
    
    private static void merging(Tuple<Vector2, double>[] BuildingsWithDistance, int left, int mid, int right){
        //List<Tuple<Vector2, double>> tempList = new List<Tuple<Vector2, double>>(50);
        Tuple<Vector2, double>[] tempList = new Tuple<Vector2, double>[50];

        int leftRange = (mid - 1);
        int tmp_pos = left;
        int num_elements = (right - left + 1);

        while (left <= leftRange && (mid <= right)){ 
            if(BuildingsWithDistance[left] != null || BuildingsWithDistance[mid] != null){
                if (BuildingsWithDistance[left].Item2 <= BuildingsWithDistance[mid].Item2) {
                    tempList[tmp_pos++] = BuildingsWithDistance[left++];
                }
                else {
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
