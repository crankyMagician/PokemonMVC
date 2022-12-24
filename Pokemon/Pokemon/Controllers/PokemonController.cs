using Microsoft.AspNetCore.Mvc;
using Pokemon.Models;
using System.IO;
using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Collections;
using MockQueryable.Moq;
using System;
using Microsoft.Data.SqlClient;

namespace Pokemon;
//namespace Pokemon.Controllers;
public class PokemonController : Controller
{
    
    //a const page size
    const int pageSize = 50;
    // a const for fuzzy search must score within 3 
    const int threshold = 3;
    //creatung a list of strings 
    private List<string> pokemonNames = new List<string>();

    //a cached list with every pokemon found  
    List<PokemonModel> pokemonList = new List<PokemonModel>();

    public async Task<IActionResult> Index(
        string sortOrder,
        string currentFilter,
        string searchString,
        int? pageNumber)
    {
 
        //a list to return for the view 
        List<PokemonModel> _pokemonList = new List<PokemonModel>();
        //debug line 
        System.Console.WriteLine($"Attempting to order.Sort order: {sortOrder}, Search string: {searchString}");
        _pokemonList = PokemonList();
        // var results = GetPaginatedResults(1, 150);
        //Filter: HP, Attack & Defense
        ViewData["HPSortParm"] = sortOrder == "" ? "hp" : "HP";
        ViewData["AttacKSortParm"] = sortOrder == "" ? "attack" : "Attack";
        ViewData["DefenseSortParm"] = sortOrder == "" ? "defense" : "Defense";
        //if search string is not null 
        if (searchString != null)
        {
            pageNumber = 1;
        }
        else
        {
            //search string is current filter 
            searchString = currentFilter;
        }

        ViewData["SearchFilter"] = searchString;
        // the pokemons in the pokemon list 
        var pokemons = from p in _pokemonList
                       select p;
        //if the string is not empty search for it 
        if (!String.IsNullOrEmpty(searchString))
        {
  
            //pokemon fuzzy search
            //iterating overstrings and adding the names to a the list 
            string? pokemonStrings = pokemonNames.FirstOrDefault(p => FuzzySearch.ComputeDistance(p, searchString) < threshold);
            System.Console.WriteLine($"Pokemon strings = {pokemonStrings}");
            //a list to return for the view 
            List<PokemonModel> _pokemonFuzzyList = new List<PokemonModel>();
            //if the names where found with fuzzy search show those pokemon to the user 
            if (pokemonStrings != null)
            {   //loop through  cached list 
                foreach (PokemonModel poke in pokemonList)
                {
                    //if the strings contain the pokemon name 
                    if (pokemonStrings.Contains(poke.PName))
                    {
                        //add the pokemon to the fuzzy list 
                        _pokemonFuzzyList.Add(poke);
                        //debug 
                        System.Console.WriteLine($"Pokemon fuzzy search {poke.PName} added to fuzzy list");
                    }
                }
                //set the pokemon list to the fuzzy list 
                _pokemonList = _pokemonFuzzyList;
            }
            //if the strings are null perform a basic search 
            else
            {
                //working search basic 
                //return a list that contains the name of the pokemon 
                pokemons= pokemons.Where(s => s.PName.Contains(searchString));
                _pokemonList = pokemons.ToList();

            }
        }
        switch (sortOrder)
        {
           
            case "HP":
                _pokemonList = _pokemonList.OrderBy(s => s.HP).ToList();
                System.Console.WriteLine($"Ordered by hp.");
                // Define a variable to store the sort order     
                break;
            case "Attack":
                _pokemonList = _pokemonList.OrderBy(s => s.Attack).ToList();
                System.Console.WriteLine($"Ordered by attack.");
                break;
            case "Defense":
                _pokemonList = _pokemonList.OrderBy(s => s.Defense).ToList();
                System.Console.WriteLine($"Ordered by defense.");
                break;
            default:
                System.Console.WriteLine($"Default occurence.");
                break;
        }
        
        //2 - build mock by extension to make csv queryable
        //this library is good for mock dbs: https://github.com/romantitov/MockQueryable
        var mock = _pokemonList.BuildMock();
        //return a paginated view using create async method 
        return View(await PaginatedList<PokemonModel>.CreateAsync(mock, pageNumber ?? 1, pageSize));
     
    }

    //the action result
    public ActionResult Search(string hp, string attack, string defense)
    {
        //a list to return for the view 
        List<PokemonModel> _pokemonList = new List<PokemonModel>();
        //debug line 
       
        _pokemonList = PokemonList();
        System.Console.WriteLine($"List count from search: {pokemonList.Count.ToString()}");
        // Declare a nullable integer variable to store the parsed value
        // If the query string is null, assign null to the  variable
        // Otherwise, parse the "hp" query string as an integer and assign it to the 
        int? hpInt = hp == null ? (int?)null : int.Parse(hp);
        int? attackInt = attack == null ? (int?)null : int.Parse(attack);
        int? defenseInt = defense == null ? (int?)null : int.Parse(defense);


        //working search basic 
        //return a list that contains the name of the pokemon 
        //var pokemons = pokemonList.Where(s => s.HP == hpInt);
        //_pokemonList = pokemonList.Where(s => s.HP == hpInt).ToList() ;
        //_pokemonList = pokemons.ToList();
        

        // Use LINQ's Where method to filter the collection of Pokemon based on the values of the hpInt, attackInt, and defenseInt variables
        var filteredPokemon = pokemonList.Where(p =>
            // If the hpInt variable is null, include all Pokemon
            // Otherwise, include only Pokemon with a matching HP value
            (hpInt == null || p.HP == hpInt) &&
            // If the attackInt variable is null, include all Pokemon
            // Otherwise, include only Pokemon with a matching Attack value
            (attackInt == null || p.Attack == attackInt) &&
            // If the defenseInt variable is null, include all Pokemon
            // Otherwise, include only Pokemon with a matching Defense value
            (defenseInt == null || p.Defense == defenseInt));

        _pokemonList = filteredPokemon.ToList();
       // List<PokemonModel> list = pokemons.ToList();
        System.Console.WriteLine($"HP {hp}. Attack {attack}. Defense {defense}. List count: {_pokemonList.Count.ToString()}");
     

        // Search for Pokemon using the form values
        // ...
       
        return View(_pokemonList);
    }

    //set the list from the csv file 
    public List<PokemonModel> PokemonList()
    {
        //the list of pokemon
        List<PokemonModel> _pokemonList = new List<PokemonModel>();
        //read the stream 
        using (StreamReader reader = new StreamReader("Data/pokemon.csv"))
        {
            
            string line;
            reader.ReadLine();
            //while there are still lines keep reading 
            while ((line = reader.ReadLine()) != null)
            {
                
                //split the lines on every comma
                string[] fields = line.Split(',');
                //the id field 
                int id = int.Parse(fields[0].Trim());
                // the name field 
                string name = fields[1].Trim();
                //type one 
                string type1 = fields[2].Trim();
                //type two 
                string type2 = fields[3].Trim();
                //total
                int total = int.Parse(fields[4].Trim());
                //hp
                int hp = int.Parse(fields[5].Trim());
                //attack
                int attack = int.Parse(fields[6].Trim());
                //defense
                int defense = int.Parse(fields[7].Trim());
                //sp atk
                int spAtk = int.Parse(fields[8].Trim());
                //sp def
                int spDef = int.Parse(fields[9].Trim());
                //gen
                int gen = int.Parse(fields[10].Trim());
                //speed
                int speed = int.Parse(fields[11].Trim());
                //if it is legendary
                bool legendary = fields[12].Trim().ToLower() == "true";
                //if it is legendary 
                if (legendary)
                {
                    //System.Console.WriteLine($"Legendary detected. The name of the pokemon is {name}");
                    //continue to not include legendary
                    continue;
                }
                //if it is a ghost 
                if (type1 == "Ghost" || type2 == "Ghost")
                {
                   // System.Console.WriteLine($"Ghost detected. The name of the pokemon is {name}");
                    //continue to not include ghost
                    continue;
                }
                //if steel type doulbe hp
                if (type1 == "Steel" || type2 == "Steel")
                {
                    hp *= 2;
                    //System.Console.WriteLine($"Steel type detected. The name of the pokemon is {name}. The new hp is: {hp}");
                }
                //if fire type reduce by 10%
                if (type1 == "Fire" || type2 == "Fire")
                {
                    attack = (int)(attack * 0.9);
                   // System.Console.WriteLine($"Fire type detected. The name of the pokemon is {name}. The new attack is: {attack}");
                }
                //if bug or flying type increased by 10%
                if ((type1 == "Bug" || type2 == "Bug") && (type1 == "Flying" || type2 == "Flying"))
                {
                    speed = (int)(speed * 1.1);
                  //  System.Console.WriteLine($"Bug or flying type detected. The name of the pokemon is {name}. The new speed is: {speed}. The type one was {type1}, type two was {type2} ");
                }
                
                //if the name starts with a G increase for every value thats not a g 
                if (name[0] == 'G')
                {
                    //for debug purposes 
                    int oldDef = defense;
                    //the total 
                    int _total = name.Count(c => c != 'G');
                    //the value to increase *5
                    int valueToIncrease = _total * 5;
                    //the defense 
                    defense += valueToIncrease;
                    System.Console.WriteLine($"Pokemon's name begins with a G. The name of the pokemon is {name}. " +
                        $" The new defense is: {defense}" +
                        $" The total amount of letters that are not g are: {_total}" +
                        $" The value increased is: {valueToIncrease}" +
                        $" The old defense was: {oldDef}");
                }
                //set the pokemon from the csv 
                PokemonModel pokemon = new PokemonModel
                {
                    Id = id,
                    PName = name,
                    TypeOne = type1,
                    TypeTwo = type2,
                    HP = hp,
                    Attack = attack,
                    Defense = defense,
                    Speed = speed,
                    SpDefense = spDef,
                    SpAttack = spAtk,
                    Gen = gen,
                    Total = total
                };
                //add it to the list 
                _pokemonList.Add(pokemon);
            }
            //adding all the names to this list for searching function 
            for (int i = 0; i < _pokemonList.Count; i++)
            {
                pokemonNames.Add(_pokemonList[i].PName);
            }
            //return the list ordered by id
            /*
             * I added this in because I felt like it made it easier
             * to check the output against the csv
             */
            //caching the complete list in case I need it 
            pokemonList = _pokemonList;
            System.Console.WriteLine($" List count: {pokemonList.Count.ToString()}");
            return OrderListByID(_pokemonList);
        }

    

    }

    //return an ordered list by ID 
    private static List<PokemonModel> OrderListByID(List<PokemonModel> list)
    {


        List<PokemonModel> orderedList = list.OrderBy(p => p.Id).ToList();


        return orderedList;
    }
}


 

