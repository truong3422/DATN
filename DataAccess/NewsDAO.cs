using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class NewsDAO
    {
        private readonly poolcomvnContext _context;
        public NewsDAO(poolcomvnContext context)
        {
            _context = context;
        }
        public void AddNews(News news)
        {
            if (news == null)
            {
                throw new ArgumentNullException(nameof(news));
            }

            if (string.IsNullOrWhiteSpace(news.Title))
            {
                throw new ArgumentException("News title cannot be null or empty", nameof(news.Title));
            }

            if (string.IsNullOrWhiteSpace(news.Description))
            {
                throw new ArgumentException("News description cannot be null or empty", nameof(news.Description));
            }

            _context.News.Add(news);
            _context.SaveChanges();
        }
        public void ChangeNewsStatus(int id)
        {
            var newsToUpdate = _context.News.FirstOrDefault( c=> c.NewsId==id); 
            if (newsToUpdate != null)
            {
                if (newsToUpdate.Status == true) { newsToUpdate.Status = false; } else { newsToUpdate.Status = true; }; 
                _context.SaveChanges();
            }
        }
        public News GetNewsById(int newsId)
        {
            try
            {
                return _context.News
                    .Include(n => n.Acc)
                    .FirstOrDefault(n => n.NewsId == newsId);
            }
            catch (Exception ex)
            {
               
                throw new Exception($"Error while retrieving news with ID {newsId}.", ex);
            }
        }
        public List<News> GetAllNews()
        {
            try
            {
                return _context.News
                    .Include(n => n.Acc)
                    .ToList();
            }
            catch (Exception ex)
            {
               
                throw new Exception("Error while retrieving all news.", ex);
            }
        }

        public List<News> GetNewsByTitle(string searchQuery)
        {
            try
            {
                return _context.News
                    .Include(n => n.Acc)
                    .Where(n => n.Title.Contains(searchQuery))
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while retrieving news with title containing '{searchQuery}'.", ex);
            }
        }


        public List<News> GetLatestNews()
        {
            try
            {
                return _context.News
                    .Include(n => n.Acc)
                    .OrderByDescending(n => n.CreatedDate)
                    .ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error while retrieving the latest news.", ex);
            }
        }

        public void UpdateNews(News updatedNews)
        {
            if (updatedNews == null)
            {
                throw new ArgumentNullException(nameof(updatedNews));
            }

            if (string.IsNullOrWhiteSpace(updatedNews.Title))
            {
                throw new ArgumentException("Updated news title cannot be null or empty", nameof(updatedNews.Title));
            }

            if (string.IsNullOrWhiteSpace(updatedNews.Description))
            {
                throw new ArgumentException("Updated news description cannot be null or empty", nameof(updatedNews.Description));
            }

            var existingNews = _context.News.FirstOrDefault(u => u.NewsId == updatedNews.NewsId);

            if (existingNews != null)
            {
                existingNews.Title = updatedNews.Title;
                existingNews.Description = updatedNews.Description;
                existingNews.UpdatedDate = DateTime.Now;

                _context.SaveChanges();
            }
        }
        public Account GetAccount(int AccID)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.AccountId == AccID);
            return account;
        }
        public void DeleteNews(int newsId)
        {
            if (newsId <= 0)
            {
                throw new ArgumentException("News ID must be greater than 0", nameof(newsId));
            }

            var newsToDelete = _context.News.FirstOrDefault(u => u.NewsId == newsId);

            if (newsToDelete != null)
            {
                _context.News.Remove(newsToDelete);
                _context.SaveChanges();
            }
        }

    }
}
