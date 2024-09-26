using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class SoloMatchDAO
    {
        private readonly poolcomvnContext _context;
        public SoloMatchDAO(poolcomvnContext context)
        {
            _context = context;
        }
        public void AddSoloMatch(SoloMatch soloMatch)
        {
            _context.SoloMatches.Add(soloMatch);
            _context.SaveChanges();
        }
        public SoloMatch GetSoloMatchById(int solomatchId)
        {
            return _context.SoloMatches.FirstOrDefault(p => p.SoloMatchId == solomatchId);
        }
        public List<SoloMatch> GetAllSoloMatchByClubID(int clubID)
        {
            return _context.SoloMatches.Where(s => s.ClubId == clubID).ToList();
        }
        public SoloMatch GetLastestSoloMatch()
        {
            try
            {
                var soloMatch = _context.SoloMatches.OrderByDescending(e => e.SoloMatchId).FirstOrDefault();

                return soloMatch;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<Player> GetPlayerForSoloMatch(int soloMatchId)
        {
            var soloMatch = _context.SoloMatches
                .Include(sm => sm.PlayerInSoloMatches) 
                    .ThenInclude(psm => psm.Player)   
                .FirstOrDefault(sm => sm.SoloMatchId == soloMatchId);

            if (soloMatch == null)
            {
                return null;
            }
            var playerList = new List<Player>();
            foreach (var playerInMatch in soloMatch.PlayerInSoloMatches)
            {
                var player = playerInMatch.Player;
                
                playerList.Add(player);
            }
            return playerList;
        }

        public PlayerInSoloMatch GetScorePLayerForMatch(int soloMatchId, int playerId)
        {
            try
            {
                return _context.PlayerInSoloMatches.FirstOrDefault(p => p.SoloMatchId == soloMatchId && p.PlayerId == playerId);
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public void UpdateScore(PlayerInSoloMatch player1)
        {
            try
            {
               
              _context.PlayerInSoloMatches.Update(player1);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }

        public PlayerInSoloMatch GetPlayerInMatch(int playerSoloMatchId)
        {
            try
            {
                return _context.PlayerInSoloMatches.FirstOrDefault(p => p.PlayerSoloMatchId == playerSoloMatchId);
            }
            catch (Exception ex)
            {

                throw ex;
            };
        }

        public void FinishMatch(SoloMatch solomatch)
        {
            try
            {
                _context.SoloMatches.Update(solomatch);
                _context.SaveChanges();
            }
            catch (Exception e)
            {

                throw e;
            }
            
        }
    }
}
