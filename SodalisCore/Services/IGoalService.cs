﻿using System.Threading.Tasks;
using SodalisDatabase.Entities;

namespace SodalisCore.Services {
    public interface IGoalService {
        internal Task<Goal> GetGoalById(int goalId);
        internal Task<Goal[]> GetGoalsByUserId(int userId);
        internal Task<Goal> CreateGoal(Goal goal);
        internal Task<Goal> UpdateGoal(int id, Goal goal);
        internal Task DeleteGoal(int id);
    }
}