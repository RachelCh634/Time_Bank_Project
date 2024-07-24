﻿using AutoMapper;
using DAL.DTO;
using DAL.Interfaces;
using Microsoft.EntityFrameworkCore;
using MODELS.Models;
using Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Data
{
    public class UserData : IUserData
    {
        private readonly DBContext _context;
        private readonly IMapper _mapper;

        public UserData(DBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<User>> GetAllUsers()
        {
            var users = await _context.Users.ToListAsync();
            return users;
        }

        public async Task<User> GetUserById(string id)
        {
            var user = await _context.Users.FindAsync(id);
            return user;
        }

        public async Task<bool> AddUser(UserDto userDto)
        {
            string userId = userDto.Id;

            if (!IsValidIsraeliId(userId))
            {
                return false;
            }

            string role = !_context.Users.Any() ? "Admin" : "User";

            var userEntity = _mapper.Map<User>(userDto);
            userEntity.Role = role;

            _context.Users.Add(userEntity);

            int changes = await _context.SaveChangesAsync();

            return changes > 0;
        }

        public async Task<bool> AddAdmin(UserDto userDto)
        {
            string userId = userDto.Id;

            if (!IsValidIsraeliId(userId))
            {
                return false;
            }

            var userEntity = _mapper.Map<User>(userDto);
            userEntity.Role = "Admin";

            _context.Users.Add(userEntity);

            int changes = await _context.SaveChangesAsync();

            return changes > 0;
        }

        public async Task<bool> IsUserAdmin(string id)
        {
            var user = await _context.Users.FindAsync(id);
            return user != null && user.Role == "Admin";
        }

        private bool IsValidIsraeliId(string id)
        {
            if (id.Length != 9 || !id.All(char.IsDigit))
            {
                return false;
            }

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                int num = int.Parse(id[i].ToString());
                int weight = i % 2 == 0 ? num : num * 2;

                sum += weight > 9 ? weight - 9 : weight;
            }

            return sum % 10 == 0;
        }

        public async Task<bool> AddHoursDonation(int hours, string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }
            user.HoursDonation += hours;
            int changes = await _context.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> RemoveHoursAvailable(int hours, string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }
            if ((user.HoursAvailable - hours) < 0)
                return false;
            user.HoursAvailable -= hours;
            int changes = await _context.SaveChangesAsync();
            return changes > 0;
        }

        public async Task<bool> DeleteUser(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }

            var donations = _context.Donations.Where(d => d.DonorId.ToString() == id);
            _context.Donations.RemoveRange(donations);

            _context.Users.Remove(user);

            int changes = await _context.SaveChangesAsync();
            return changes > 0;
        }
    }
}
