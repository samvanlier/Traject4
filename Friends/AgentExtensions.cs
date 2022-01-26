using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Graph;

namespace Friends
{
    internal static class AgentExtensions
    {
        #region FoaF

        public static int FoaF(this Agent agent)
            => agent.FoaF(new Random());

        public static int FoaF(this Agent agent, int seed)
            => agent.FoaF(new Random(seed));

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        public static int FoaF(this Agent agent, Random random)
        {
            var friends = agent.Friends;
            var knownAgents = friends.Select(a => a.Id).ToList();
            knownAgents.Add(agent.Id); // add self so that the other agent doesn't suggest yourself as a new friend

            double totalFoaF = friends.Select(a => a.Friends.Count).Sum();

            var chances = friends.Select(a => (double)a.Friends.Count / totalFoaF).ToList();

            Debug.WriteLine($"totalFoaF={totalFoaF}\t" +
                $"chances=[{string.Join(",", chances)}]");
            var friend = random.Choice(friends, chances);

            var newFriend = friend.SuggestFriend(knownAgents, random);
            if (newFriend == null)
                return -1;

            agent.Friends.Add(newFriend);
            newFriend.Friends.Add(agent);
            return newFriend.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="knownAgents"></param>
        /// <param name="random"></param>
        /// <returns></returns>
        private static Agent SuggestFriend(this Agent agent, ICollection<int> knownAgents, Random random)
        {
            var options = agent.Friends.Where(a => !knownAgents.Contains(a.Id)).ToList();

            if (options.Count == 0)
                return null; // no new friend to add...

            var i = random.Next(options.Count);
            var f = options.ElementAt(i);

            return f;
        }

        #endregion

        #region Remove Friend

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="success"></param>
        /// <param name="random"></param>
        public static int RemoveFriend(this Agent agent, ICollection<int> success, int totalSuccess, Random random)
        {
            Agent enemy;
            do
            {
                if (totalSuccess == 0)
                {
                    int i = random.Next(agent.Friends.Count);
                    enemy = agent.Friends.ElementAt(i);
                }
                else
                {
                    var chances = success.Select(i => (double)i / totalSuccess);
                    enemy = random.Choice(agent.Friends, chances);
                }
            } while (enemy.Friends.Count < 2);

            agent.RemoveFriend(enemy);
            enemy.RemoveFriend(agent);

            return enemy.Id;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="enemy"></param>
        private static void RemoveFriend(this Agent agent, Agent enemy)
        {
            agent.Friends.Remove(enemy);
            //agent.Enemies.Add(enemy);
        }

        #endregion
    }
}
