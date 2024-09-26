using BusinessObject.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess
{
    public class TableDAO
    {
        private readonly poolcomvnContext _context;

        public TableDAO(poolcomvnContext poolComContext)
        {
            _context = poolComContext;
        }
        public List<Table> GetAllTablesForClub(int clubId)
        {
            return _context.Tables.Where(t => t.ClubId == clubId).ToList();
        }

        // Create
        public void AddTable(Table table)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));

            }
            table.IsScheduling = false;
            _context.Tables.Add(table);
            _context.SaveChanges();
        }

        // Read
        public Table GetTableById(int tableId)
        {
            return _context.Tables.Find(tableId);
        }

        public List<Table> GetAllTables()
        {
            return _context.Tables.ToList();
        }

        public List<Table> SearchTables(string searchQuery)
        {
            searchQuery.ToLower();
            return _context.Tables.Where(t => t.TableName.Contains(searchQuery)).ToList();
        }

        // Update
        public void UpdateTable(Table updatedTable)
        {
            var existingTable = _context.Tables.Find(updatedTable.TableId);

            if (existingTable != null)
            {
                // Update properties as needed
                existingTable.TableName = updatedTable.TableName;
                existingTable.ClubId = updatedTable.ClubId;
                existingTable.TagName = updatedTable.TagName;
                existingTable.Status = updatedTable.Status;
                existingTable.Size = updatedTable.Size;
                existingTable.Image = updatedTable.Image;
                existingTable.IsScheduling = updatedTable.IsScheduling;
                existingTable.IsUseInTour = updatedTable.IsUseInTour;

                _context.SaveChanges();
            }
        }

        public void UpdateIsUseInTourStatus(List<int> tableIds, bool isUseInTour)
        {
            // Retrieve the tables to be updated
            var tablesToUpdate = _context.Tables.Where(t => tableIds.Contains(t.TableId));

            // Update the IsUseInTour property
            foreach (var table in tablesToUpdate)
            {
                table.IsUseInTour = isUseInTour;
            }

            // Save changes to the database
            _context.SaveChanges();
        }

        // Delete
        public void DeleteTable(int tableId)
        {
            var tableToDelete = _context.Tables.Find(tableId);

            if (tableToDelete != null)
            {
                _context.Tables.Remove(tableToDelete);
                _context.SaveChanges();
            }
        }

        public void AddTableToTournament(List<int> lstTableId)
        {
            foreach (int i in lstTableId)
            {
                var table = _context.Tables.FirstOrDefault(t => t.TableId == i);
                table.IsUseInTour = true;
                _context.Update(table);
            }
            _context.SaveChanges();
        }

        public void RemoveTableFromTournament(List<int> lstTableId)
        {
            foreach (int i in lstTableId)
            {
                var table = _context.Tables.FirstOrDefault(t => t.TableId == i);
                table.IsUseInTour = false;
                _context.Update(table);
            }
            _context.SaveChanges();
        }

        public List<Table> TableNotInSchedule(int cludId)
        {
            try
            {
                return _context.Tables.Where(t => t.IsScheduling == false && t.ClubId == cludId 
                                                && t.IsUseInTour == true).ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public List<Table> TableNotInUse(int cludId)
        {
            try
            {
                return _context.Tables.Where(t => t.Status == Constant.TableNotUse && t.ClubId == cludId
                                                && t.IsUseInTour == true).ToList();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public bool IsTagNameExistsAsync(int clubId, string tagName)
        {
            if (string.IsNullOrEmpty(tagName))
            {
                throw new ArgumentNullException(nameof(tagName));
            }
            return _context.Tables.Any(t => t.ClubId == clubId && t.TagName == tagName);
        }
        public void AddListTable(List<Table> tables)
        {
            if (tables == null || !tables.Any())
            {
                throw new ArgumentException("No tables provided.");
            }

            _context.Tables.AddRange(tables);
            _context.SaveChanges();
        }

        public int GetNumberOfTableEmpty(int clubId)
        {
            try
            {
                int number = _context.Tables.Where(t => t.ClubId == clubId 
                    && t.Status == Constant.TableNotUse && 
                    (t.IsUseInTour != null && t.IsUseInTour == true)).Count();
                return number;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int GetNumberOfTablePlaying(int clubId)
        {
            try
            {
                int number = _context.Tables.Where(t => t.ClubId == clubId 
                    && t.Status == Constant.TableInUse && 
                    (t.IsUseInTour != null && t.IsUseInTour == true)).Count();
                return number;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public int GetNumberOfTableUseInTour(int clubId)
        {
            try
            {
                int number = _context.Tables.Where(t => t.ClubId == clubId
                    && (t.IsUseInTour != null && t.IsUseInTour == true)).Count();
                return number;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
