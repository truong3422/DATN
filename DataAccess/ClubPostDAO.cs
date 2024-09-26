using BusinessObject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class ClubPostDAO
    {
        private readonly poolcomvnContext _context;

        public ClubPostDAO(poolcomvnContext context)
        {
            _context = context;
        }

        // Create ClubPost
        public void AddClubPost(ClubPost clubPost)
        {
            _context.ClubPosts.Add(clubPost);
            _context.SaveChanges();
        }

        // Read ClubPost
        public ClubPost GetClubPostById(int postId)
        {
            return _context.ClubPosts.FirstOrDefault(p=>p.PostId==postId);
        }
        public List<ClubPost> GetClubPostByClubId(int clubId)
        {
            return _context.ClubPosts.Where(cp => cp.ClubId == clubId).ToList();
            ;
        }

        public List<ClubPost> GetAllClubPosts()
        {
            return _context.ClubPosts.ToList();
        }

        public List<ClubPost> SearchClubPostsByTitleAndClubId(string title, int clubId)
        {
            return _context.ClubPosts.Where(cp => cp.Title.Contains(title) && cp.ClubId == clubId).ToList();
        }

        public void UpdateClubPost(ClubPost updatedClubPost)
        {
            var existingClubPost = _context.ClubPosts.FirstOrDefault(p => p.PostId == updatedClubPost.PostId);

            if (existingClubPost != null)
            {
                existingClubPost.Title = updatedClubPost.Title;
                existingClubPost.Description = updatedClubPost.Description;
                existingClubPost.CreatedDate = updatedClubPost.CreatedDate;
                existingClubPost.UpdatedDate = updatedClubPost.UpdatedDate;
                existingClubPost.Link = updatedClubPost.Link;
                existingClubPost.Flyer = updatedClubPost?.Flyer;
                existingClubPost.Status = updatedClubPost?.Status;
                _context.SaveChanges();
            }
        }


        // Delete ClubPost
        public void DeleteClubPost(int postId)
        {
            var postToDelete = _context.ClubPosts.Find(postId);

            if (postToDelete != null)
            {
                _context.ClubPosts.Remove(postToDelete);
                _context.SaveChanges();
            }
        }
    }
}

