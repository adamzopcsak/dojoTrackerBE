﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DojoTracker.Models;
using DojoTracker.Services.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DojoTracker.Services.Repositories
{
    public class SolutionRepository : ISolutionRepository
    {
        private readonly DojoTrackerDbContext _context;
        private readonly UserManager<User> _userManager;

        public SolutionRepository(DojoTrackerDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task AddSolutionAsync(Solution solution, string userId)
        {
            solution.UserId = userId;
            solution.SubmissionDate = DateTime.UtcNow;
            solution.SubmissionDate = solution.SubmissionDate.AddHours(2);

            var result = FindSolution(solution).Result;

            if (result != null)
            {
                result.Code = solution.Code;
            }
            else
            {
                _context.Solutions.Add(solution);

                var dojoScore = await GetScoreByDojoIdAsync(solution.DojoId);
                await UpdateUserScoreOnSubmitAsync(dojoScore, userId, solution.DojoId);
            }

            await _context.SaveChangesAsync();
        }

        public IQueryable<Solution> ListSolutionsByDojoId(int dojoId)
        {
            return  _context.Solutions.Where(solution => solution.DojoId == dojoId);
        }

        public async Task<IEnumerable<Solution>> ListSolutionsByUserIdAsync(string userId)
        {
            return await _context.Solutions.Where(solution => solution.UserId == userId)
                .ToListAsync();
        }

        public async Task<Solution> GetUserSolutionByDojoIdAsync(int id, string userId, string language)
        {
            return await _context.Solutions.FirstOrDefaultAsync(solution => solution.UserId == userId &&
                                                                            solution.DojoId == id &&
                                                                            solution.Language == language);

        }

        public async Task<IEnumerable<int>> ListSolvedDojoIdsByUserIdAsync(string userId)
        {
            return await _context.Solutions.Where(solution => solution.UserId == userId)
                .Select(solution => solution.DojoId).Distinct().ToListAsync();
        }

        public async Task<DateTime> GetLastCompletedByUserIdAsync(string userId)
        {
            var solutions = await ListSolutionsByUserIdAsync(userId);

            return solutions.Select(solution => solution.SubmissionDate).DefaultIfEmpty().Max();
        }

        public async Task<IEnumerable<string>> ListUserIdsByDojoIdAsync(int dojoId)
        {
            return await ListSolutionsByDojoId(dojoId).Select(solution => solution.UserId).Distinct().ToListAsync();
        }

        public IQueryable<int> ListAllDojoIdsWithASolution()
        {
            return _context.Solutions.Select(solution => solution.DojoId).Distinct();
        }

        public async Task DeleteSolution(int dojoId, string language, string userId)
        {
            var solutionToRemove = await GetUserSolutionByDojoIdAsync(dojoId, userId, language);

            _context.Solutions.Remove(solutionToRemove);
            
            await _context.SaveChangesAsync();
            
            var score = await GetScoreByDojoIdAsync(dojoId);

            await UpdateUserScoreOnSubmitAsync(Math.Abs(score) * -1, userId, dojoId);
        }

        public async Task<IEnumerable<Solution>> ListAllSolutionsForDojoByUserIdAsync(string userId, int dojoId)
        {
            return await _context.Solutions.Where(solution => solution.UserId == userId && solution.DojoId == dojoId)
                .ToListAsync();
        }


        private async Task<Solution> FindSolution(Solution solution)
        {
            return await _context.Solutions.SingleOrDefaultAsync(s => s.UserId == solution.UserId &&
                                                                      s.DojoId == solution.DojoId &&
                                                                      s.Language == solution.Language);
        }

        private async Task UpdateUserScoreOnSubmitAsync(int score, string userId, int dojoId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var solutionsForDojo = await ListAllSolutionsForDojoByUserIdAsync(userId, dojoId);

            if (!solutionsForDojo.Any())
            {
                user.Score += score;

                await _userManager.UpdateAsync(user);
            }
        }

        private async Task<int> GetScoreByDojoIdAsync(int dojoId)
        {
            return (await _context.Dojos.FirstOrDefaultAsync(dojo => dojo.Id == dojoId)).Difficulty;
        }
        
    }
}
