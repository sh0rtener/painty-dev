﻿using Communication.BusinessLayer.Contracts;
using Communication.BusinessLayer.Exceptions;
using Communication.BusinessLayer.Exceptions.ServerExceptions;
using Communication.DomainLayer.Contracts;
using Communication.DomainLayer.Dtos;
using Communication.DomainLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Communication.BusinessLayer.Services
{
    public class ImageBusinessService : IImageBusinessService
    {
        private readonly IImageService _imageService;
        private readonly IUserService _userService;
        private readonly IImageUploader _uploader;
        private readonly IConfiguration _configuration;

        public ImageBusinessService(IImageService imageService, IUserService userService,
            IImageUploader uploader, IConfiguration configuration)
        {
            _imageService = imageService;
            _userService = userService;
            _uploader = uploader; 
            _configuration = configuration;
        }

        public async Task<IReadOnlyCollection<Image>> GetFriendImagesAsync(Guid userId, Guid friendId)
        {
            User? currentUser = await _userService.GetAsync(userId);
            User? friendUser = await _userService.GetAsync(friendId);

            if (currentUser is null || friendUser is null) throw new NotFoundException<User>();

            List<Friendship> friendships = currentUser.Friends.Where(
                f => f.Approved && 
                f.RequestToId == friendId &&
                f.RequestFromId == userId
                ).ToList();

            if (friendships.Count <= 0)
                throw new InvalidDataException<User>();

            return friendUser.Images;
        }

        public async Task<IReadOnlyCollection<Image>> GetUserImagesAsync(Guid userId)
        {
            User? currentUser = await _userService.GetAsync(userId);

            if (currentUser is null) throw new NotFoundException<User>();

            List<Image> images = currentUser.Images.Where(f => f.User!.Id == userId).ToList();

            return images;
        }

        public async Task UploadAsync(IFormFile file, Guid userId)
        {
            User? user = await _userService.GetAsync(userId);
            if (user is null) throw new NotFoundException<User>();

            string path = _configuration["Image:UploadPath"]! + userId + "/";
            string filename = Guid.NewGuid() + ".jpg";

            if (!_uploader.Upload(file, path, filename))
                throw new InternalServerException("The file doesn't load");

            Image image = new Image(userId + "/" + filename, user);
            await _imageService.CreateAsync(image);
        }
    }
}
