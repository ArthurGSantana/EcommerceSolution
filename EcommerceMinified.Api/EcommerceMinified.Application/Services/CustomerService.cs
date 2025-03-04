using System;
using AutoMapper;
using EcommerceMinified.Domain.Entity;
using EcommerceMinified.Domain.Enum;
using EcommerceMinified.Domain.Exceptions;
using EcommerceMinified.Domain.Interfaces.Repository;
using EcommerceMinified.Domain.Interfaces.Services;
using EcommerceMinified.Domain.ViewModel.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EcommerceMinified.Application.Services;

public class CustomerService(IUnitOfWork _unitOfWork, IMapper _mapper) : ICustomerService
{
    public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customer)
    {
        var exists = await _unitOfWork.CustomerRepository.GetAsync(false, null, x => x.Email == customer.Email);

        if (exists != null)
        {
            throw new EcommerceMinifiedDomainException("Customer already exists", ErrorCodeEnum.AlreadyExists);
        }

        var newCustomer = _mapper.Map<Customer>(customer);

        _unitOfWork.CustomerRepository.Insert(newCustomer);
        await _unitOfWork.CommitPostgresAsync();

        return _mapper.Map<CustomerDto>(newCustomer);
    }

    public async Task DeleteCustomerAsync(Guid id)
    {
        var customer = await _unitOfWork.CustomerRepository.GetAsync(false, null, x => x.Id == id);

        if (customer == null)
        {
            throw new EcommerceMinifiedDomainException("Customer not found", ErrorCodeEnum.NotFound);
        }

        _unitOfWork.CustomerRepository.Delete(customer);
        await _unitOfWork.CommitPostgresAsync();
    }

    public async Task<Customer> GetCustomerByIdAsync(Guid id)
    {
        var customer = await _unitOfWork.CustomerRepository.GetAsync(false, x => x.Include(c => c.Address), x => x.Id == id);

        if (customer == null)
        {
            throw new EcommerceMinifiedDomainException("Customer not found", ErrorCodeEnum.NotFound);
        }

        return customer;
    }

    public async Task<List<Customer>> GetCustomersAsync()
    {
        return await _unitOfWork.CustomerRepository.GetFilteredAsync(false, x => x.Include(c => c.Address));
    }

    public async Task<CustomerDto> UpdateCustomerAsync(CustomerDto customer)
    {
        var currentCustomer = await _unitOfWork.CustomerRepository.GetAsync(true, null, x => x.Id == customer.Id);

        if (currentCustomer == null)
        {
            throw new EcommerceMinifiedDomainException("Customer not found", ErrorCodeEnum.NotFound);
        }

        currentCustomer.Name = customer.Name;
        currentCustomer.Email = customer.Email;
        currentCustomer.Phone = customer.Phone;
        currentCustomer.Image = customer.Image;

        _unitOfWork.CustomerRepository.Update(currentCustomer);
        await _unitOfWork.CommitPostgresAsync();

        return _mapper.Map<CustomerDto>(currentCustomer);
    }
}
