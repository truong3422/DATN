using BusinessObject.Models;
using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace DataAccess
{
    public class PlayerDAO
    {
        private readonly poolcomvnContext _context;
        //public List<PlayerDTO> ProcessedPlayers { get; set; } = new List<PlayerDTO>();
        public PlayerDAO(poolcomvnContext context)
        {
            _context = context;
        }

        public void AddPlayer(Player player)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            _context.Players.Add(player);
            _context.SaveChanges();
        }

       
        public Player GetPlayerById(int playerId)
        {
            return _context.Players.Find(playerId);
        }

        public Player GetPlayerByUserIdAndTourId(int userId, int tourId)
        {
            Player player = _context.Players.FirstOrDefault(x => x.UserId == userId && x.TourId == tourId);
            return player;
        }

        public string RemovePlayerByUserIdAndTourId(int userId, int tourId)
        {
            Player player = _context.Players.FirstOrDefault(x=> x.UserId == userId &&  x.TourId == tourId);
            string playerReturn = player.PlayerName;

            _context.Players.Remove(player);
            _context.SaveChanges();
            return playerReturn;
        }

       
        public Player GetPlayerByName(string playerName)
        {
            return _context.Players.FirstOrDefault(p => p.PlayerName == playerName);
        }


        
        public List<Player> GetAllPlayers()
        {
            return _context.Players.Include(player => player.User)
                    .ThenInclude(user => user.Account)
                .Include(player => player.Country)
                .ToList();
        }

        
        public void UpdatePlayer(Player updatedPlayer)
        {
            if (updatedPlayer == null)
            {
                throw new ArgumentNullException(nameof(updatedPlayer));
            }

            var existingPlayer = _context.Players.Find(updatedPlayer.PlayerId);

            if (existingPlayer != null)
            {
                // Update player properties
                existingPlayer.PlayerName = updatedPlayer.PlayerName;
                existingPlayer.Level = updatedPlayer.Level;

                // If User and Account properties are not null, update them
                if (updatedPlayer.User != null && updatedPlayer.User.Account != null)
                {
                    existingPlayer.User.Account.PhoneNumber = updatedPlayer.User.Account.PhoneNumber;
                }

                _context.SaveChanges();
            }
            else
            {
                
                throw new ArgumentException($"Player with ID {updatedPlayer.PlayerId} not found");
            }
        }

        
        public void DeletePlayer(int playerId)
        {
            var playerToDelete = _context.Players.Find(playerId);

            if (playerToDelete != null)
            {
                _context.Players.Remove(playerToDelete);
                _context.SaveChanges();
            }
            else
            {
                
                throw new ArgumentException($"Player with ID {playerId} not found");
            }
        }

      

        public IEnumerable<Player> GetPlayersByTournament(int tourId)
        {
            try
            {
                var players = _context.Players.Include(p => p.Country).Where(p => p.TourId == tourId
                    && (p.Status == Constant.NormalPlayer || p.Status == Constant.SurrenderPlayer
                    || p.Status == Constant.GiveUpPlayer || p.Status == Constant.EliminatedPlayer));
                return players;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public IEnumerable<PlayerInMatch> GetPlayersByMatchTour(int matchId)
        {
            try
            {
                var players = _context.PlayerInMatches.Include(p => p.Player)
                    .ThenInclude(player => player.Country)
                    .Where(p => p.MatchId == matchId);
                return players;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public int GetNumberPlayerByTourId(int tourId)
        {
            try
            {
                int numberPlayers = _context.Players.Where(p => p.TourId == tourId
                && (p.Status == Constant.NormalPlayer || p.Status == Constant.SurrenderPlayer
                    || p.Status == Constant.GiveUpPlayer || p.Status == Constant.EliminatedPlayer)).Count();
                return numberPlayers;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public void AddPlayerToMatch(int matchId, int playerId)
        {
            try
            {
                PlayerInMatch player = new PlayerInMatch()
                {
                    PlayerId = playerId,
                    MatchId = matchId,
                };
                _context.PlayerInMatches.Add(player);
                _context.SaveChanges();
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public User GetUserByID(int? userId)
        {
            return _context.Users.FirstOrDefault(u => u.UserId == userId);
        }

        public Country GetCountryByID(int? countryId)
        {
            return _context.Countries.FirstOrDefault(c => c.CountryId == countryId);
        }

        public int GetNumberPlayerExBotByTourId(int tourId)
        {
            try
            {
                int numberPlayers = _context.Players.Where(p => p.TourId == tourId
                && (p.Status == Constant.NormalPlayer || p.Status == Constant.SurrenderPlayer
                    || p.Status == Constant.GiveUpPlayer || p.Status == Constant.EliminatedPlayer)
                && !p.PlayerName.Equals("BOT")).Count();
                return numberPlayers;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public void AddPlayerToSoloMatch(PlayerInSoloMatch playerInSoloMatch)
        {
            _context.PlayerInSoloMatches.Add(playerInSoloMatch);
            _context.SaveChanges();
        }

        public List<Player> GetPlayerByUserId(int userId)
        {
            return _context.Players.Where(p => p.UserId == userId).ToList();
        }

        public Country? GetCountryByPlayerId (int playerId)
        {
            var country = _context.Players.Where(x=> x.PlayerId == playerId)
                .Select(x=> x.Country).FirstOrDefault();
            return country;
        }

        public PlayerInMatch GetPlayerInMatch(int playerId, int matchId)
        {
            return _context.PlayerInMatches.FirstOrDefault(p => p.PlayerId == playerId && p.MatchId == matchId);
        }

        public void UpdatePlayerInMatch(PlayerInMatch player)
        {
            PlayerInMatch? p = _context.PlayerInMatches.FirstOrDefault(item => item.PlayerMatchId == player.PlayerMatchId);
            if (p != null)
            {
                p.IsWinner = player.IsWinner;
                p.GameWin = player.GameWin;
                p.Score = player.Score;
                _context.Update(p);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Player> GetPlayerRequestJoinByTourId(int tourId)
        {
            try
            {
                var players = _context.Players.Include(p => p.Country).Where(p => p.TourId == tourId
                    && p.Status == null);
                return players;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public void RejectPlayer(int playerId)
        {
            try
            {
                var player = _context.Players.FirstOrDefault(p => p.PlayerId == playerId);
                player.Status = Constant.RejectedPlayer;
                _context.Players.Update(player);
                _context.SaveChanges();
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public void AcceptPlayer(int playerId)
        {
            try
            {
                var player = _context.Players.FirstOrDefault(p => p.PlayerId == playerId);
                player.Status = Constant.NormalPlayer;
                _context.Players.Update(player);
                _context.SaveChanges();
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public int GetNumberOfPlayerPlaying(int tourId)
        {
            try
            {
                int number = _context.Players.Where(p => p.TourId == tourId 
                    && p.Status == Constant.NormalPlayer).Count();
                return number;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public int GetNumberOfPlayerGiveUp(int tourId)
        {
            try
            {
                int number = _context.Players.Where(p => p.TourId == tourId
                    && p.Status == Constant.GiveUpPlayer && p.PlayerName != Constant.BotPlayerName)
                    .Count();
                return number;
            }
            catch (Exception e)
            {

                throw e;
            }
        }

        public int GetNumberOfPlayerEliminated(int tourId)
        {
            try
            {
                int number = _context.Players.Where(p => p.TourId == tourId
                    && p.Status == Constant.EliminatedPlayer).Count();
                return number;
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
