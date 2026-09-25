using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoangNV_SE1930_A01_BE.BusinessLogic.DTOs;
using HoangNV_SE1930_A01_BE.BusinessLogic.Exceptions;
using HoangNV_SE1930_A01_BE.DataAccess.Entities;
using HoangNV_SE1930_A01_BE.DataAccess.Repositories;

namespace HoangNV_SE1930_A01_BE.BusinessLogic.Services;

public class NewsArticleService : INewsArticleService
{
    private readonly INewsArticleRepository _articleRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ISystemAccountRepository _accountRepository;

    public NewsArticleService(
        INewsArticleRepository articleRepository,
        ICategoryRepository categoryRepository,
        ISystemAccountRepository accountRepository)
    {
        _articleRepository = articleRepository;
        _categoryRepository = categoryRepository;
        _accountRepository = accountRepository;
    }

    public async Task<List<NewsArticleDto>> GetAllArticlesAsync()
    {
        var articles = await _articleRepository.GetAllNewsArticlesAsync();
        var accounts = await _accountRepository.GetAllAccountsAsync();
        var accountDict = accounts.ToDictionary(a => a.AccountId, a => a.AccountName ?? "User");

        return articles.Select(a => MapToDto(a, accountDict)).ToList();
    }

    public async Task<NewsArticleDto?> GetArticleByIdAsync(string id)
    {
        var article = await _articleRepository.GetNewsArticleByIdAsync(id);
        if (article == null) return null;

        var accounts = await _accountRepository.GetAllAccountsAsync();
        var accountDict = accounts.ToDictionary(a => a.AccountId, a => a.AccountName ?? "User");

        return MapToDto(article, accountDict);
    }

    public async Task<NewsArticleDto> CreateArticleAsync(CreateNewsArticleDto dto, short currentUserId)
    {
        // Verify category exists
        var category = await _categoryRepository.GetCategoryByIdAsync(dto.CategoryId);
        if (category == null)
        {
            throw new BusinessRuleException($"Category with ID {dto.CategoryId} does not exist.");
        }

        var article = new NewsArticle
        {
            NewsArticleId = dto.NewsArticleId ?? string.Empty,
            NewsTitle = dto.NewsTitle.Trim(),
            Headline = dto.Headline.Trim(),
            NewsContent = dto.NewsContent.Trim(),
            NewsSource = dto.NewsSource?.Trim(),
            CategoryId = dto.CategoryId,
            NewsStatus = dto.NewsStatus,
            CreatedById = currentUserId,
            CreatedDate = DateTime.Now,
            UpdatedById = null,
            ModifiedDate = null
        };

        var created = await _articleRepository.AddNewsArticleAsync(article, dto.TagIds);
        return await GetArticleByIdAsync(created.NewsArticleId) ?? MapToDto(created, new Dictionary<short, string>());
    }

    public async Task<NewsArticleDto> UpdateArticleAsync(string id, UpdateNewsArticleDto dto, short currentUserId)
    {
        var existing = await _articleRepository.GetNewsArticleByIdAsync(id);
        if (existing == null)
        {
            throw new BusinessRuleException($"Article with ID '{id}' does not exist.");
        }

        var category = await _categoryRepository.GetCategoryByIdAsync(dto.CategoryId);
        if (category == null)
        {
            throw new BusinessRuleException($"Category with ID {dto.CategoryId} does not exist.");
        }

        existing.NewsTitle = dto.NewsTitle.Trim();
        existing.Headline = dto.Headline.Trim();
        existing.NewsContent = dto.NewsContent.Trim();
        existing.NewsSource = dto.NewsSource?.Trim();
        existing.CategoryId = dto.CategoryId;
        existing.NewsStatus = dto.NewsStatus;
        existing.UpdatedById = currentUserId;
        existing.ModifiedDate = DateTime.Now;

        await _articleRepository.UpdateNewsArticleAsync(existing, dto.TagIds);
        return await GetArticleByIdAsync(id) ?? MapToDto(existing, new Dictionary<short, string>());
    }

    public async Task DeleteArticleAsync(string id)
    {
        var existing = await _articleRepository.GetNewsArticleByIdAsync(id);
        if (existing == null)
        {
            throw new BusinessRuleException($"Article with ID '{id}' does not exist.");
        }

        await _articleRepository.DeleteNewsArticleAsync(id);
    }

    public async Task<NewsArticleDto> DuplicateArticleAsync(string id, short currentUserId)
    {
        var existing = await _articleRepository.GetNewsArticleByIdAsync(id);
        if (existing == null)
        {
            throw new BusinessRuleException($"Article with ID '{id}' does not exist.");
        }

        var duplicated = await _articleRepository.DuplicateNewsArticleAsync(id, currentUserId);
        if (duplicated == null)
        {
            throw new BusinessRuleException("Failed to duplicate article.");
        }

        return await GetArticleByIdAsync(duplicated.NewsArticleId) ?? MapToDto(duplicated, new Dictionary<short, string>());
    }

    public async Task<List<NewsArticleDto>> GetRelatedArticlesAsync(string id)
    {
        var article = await _articleRepository.GetNewsArticleByIdAsync(id);
        if (article == null) return new List<NewsArticleDto>();

        var tagIds = article.Tags.Select(t => t.TagId).ToList();
        var related = await _articleRepository.GetRelatedNewsArticlesAsync(id, article.CategoryId, tagIds);

        var accounts = await _accountRepository.GetAllAccountsAsync();
        var accountDict = accounts.ToDictionary(a => a.AccountId, a => a.AccountName ?? "User");

        return related.Select(a => MapToDto(a, accountDict)).ToList();
    }

    private static NewsArticleDto MapToDto(NewsArticle a, Dictionary<short, string> accountDict)
    {
        string? createdName = null;
        if (a.CreatedById.HasValue)
        {
            if (accountDict.TryGetValue(a.CreatedById.Value, out var name))
            {
                createdName = name;
            }
            else if (a.CreatedBy != null)
            {
                createdName = a.CreatedBy.AccountName;
            }
        }

        string? updatedName = null;
        if (a.UpdatedById.HasValue)
        {
            if (accountDict.TryGetValue(a.UpdatedById.Value, out var name))
            {
                updatedName = name;
            }
        }

        return new NewsArticleDto
        {
            NewsArticleId = a.NewsArticleId,
            NewsTitle = a.NewsTitle,
            Headline = a.Headline,
            CreatedDate = a.CreatedDate,
            NewsContent = a.NewsContent,
            NewsSource = a.NewsSource,
            CategoryId = a.CategoryId,
            CategoryName = a.Category?.CategoryName,
            NewsStatus = a.NewsStatus,
            CreatedById = a.CreatedById,
            CreatedByName = createdName,
            UpdatedById = a.UpdatedById,
            UpdatedByName = updatedName,
            ModifiedDate = a.ModifiedDate,
            Tags = a.Tags.Select(t => new TagDto
            {
                TagId = t.TagId,
                TagName = t.TagName,
                Note = t.Note
            }).ToList()
        };
    }
}
