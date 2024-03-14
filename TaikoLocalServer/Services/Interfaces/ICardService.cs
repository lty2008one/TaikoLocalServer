﻿using GameDatabase.Entities;
using SharedProject.Models;

namespace TaikoLocalServer.Services.Interfaces;

public interface ICardService
{
    public Task<Card?> GetCardByAccessCode(string accessCode);

    public Task<List<User>> GetUsersFromCards();

    public Task AddCard(Card card);

    public Task<bool> DeleteCard(string accessCode);
}