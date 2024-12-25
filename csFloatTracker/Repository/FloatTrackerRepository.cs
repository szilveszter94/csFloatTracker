using csFloatTracker.Context;
using csFloatTracker.Model;
using csFloatTracker.ViewModel.InternalWindows;
using Microsoft.EntityFrameworkCore;

namespace csFloatTracker.Repository;

public class FloatTrackerRepository
{
    private readonly FloatTrackerContext _context;

    public FloatTrackerRepository(FloatTrackerContext context)
    {
        _context = context;
    }

    public async Task BuyFloatAsync(InventoryItem item)
    {
        try
        {
            var account = await _context.CsAccounts.FirstOrDefaultAsync();

            if (account == null)
            {
                throw new InvalidOperationException("No account found.");
            }

            account.Inventory.Add(item);
            account.Balance -= item.Price;
            account.PurchasedCount++;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error adding FloatItem: {ex.Message}");
            throw;
        }
    }

    public async Task<CsAccount> GetAccountAsync()
    {
        try
        {
            var account = await _context.CsAccounts
                .Include(c => c.Inventory)
                .Include(c => c.TransactionHistory)
                .FirstOrDefaultAsync();

            if (account == null)
            {
                throw new InvalidOperationException("No account found.");
            }

            return account;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving Account: {ex.Message}");
            return new CsAccount();
        }
    }

    public async Task SellFloatAsync(InventoryItem item, SetSellPriceWindowVM vm)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item), "The FloatItem to remove cannot be null.");
        }

        try
        {
            var account = await _context.CsAccounts.FirstOrDefaultAsync();

            if (account == null)
            {
                return;
            }

            var inventoryItem = await _context.Inventory.FirstOrDefaultAsync(f => f.Id == item.Id);
            if (inventoryItem != null)
            {
                var sellPriceAfterTax = vm.SellPrice - (vm.SellPrice * vm.Tax / 100);
                var profit = sellPriceAfterTax - inventoryItem.Price;

                var transactionItem = new TransactionItem()
                {
                    Name = inventoryItem.Name,
                    BuyPrice = inventoryItem.Price,
                    SoldPrice = vm.SellPrice,
                    PriceAfterTax = sellPriceAfterTax,
                    Tax = vm.Tax,
                    Profit = profit,
                    CreatedDate = inventoryItem.Created,
                    Float = inventoryItem.Float,
                    CsAccountId = inventoryItem.CsAccountId
                };

                _context.TransactionHistory.Add(transactionItem);
                _context.Inventory.Remove(inventoryItem);

                account.Balance += sellPriceAfterTax;
                account.SoldCount++;
                account.Profit += profit;

                await _context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine($"FloatItem with ID {item.Id} not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing FloatItem with ID {item.Id}: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateAccountAsync(CsAccount account, EditAccountWindowVM vm)
    {
        if (account == null)
        {
            throw new ArgumentNullException(nameof(account), "The Account to edit cannot be null.");
        }

        try
        {
            var accountToEdit = await _context.CsAccounts.FirstOrDefaultAsync(a => a.Id == account.Id);

            if (accountToEdit == null)
            {
                throw new ArgumentNullException(nameof(account), "The Account to edit cannot be found.");
            }

            accountToEdit.SoldCount = vm.SoldCount;
            accountToEdit.PurchasedCount = vm.PurchasedCount;
            accountToEdit.Balance = vm.Balance;
            accountToEdit.Profit = vm.Profit;
            accountToEdit.Tax = vm.Tax;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating Account with ID {account.Id}: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateTransactionAsync(TransactionItem transactionItem, EditTransactionWindowVM vm)
    {
        if (transactionItem == null || vm == null)
        {
            throw new ArgumentNullException("The transaction item to edit cannot be null.");
        }

        try
        {
            var transactionToEdit = await _context.TransactionHistory.FirstOrDefaultAsync(a => a.Id == transactionItem.Id);

            if (transactionToEdit == null)
            {
                throw new ArgumentNullException("The transaction to edit cannot be found.");
            }

            transactionToEdit.SoldDate = vm.SellDate;
            transactionToEdit.CreatedDate = vm.BuyDate;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating transaction item with ID {transactionItem.Id}: {ex.Message}");
            throw;
        }
    }

    public async Task UpdateInventoryItemAsync(InventoryItem inventoryItem, EditFloatItemWindowVM vm)
    {
        if (inventoryItem == null)
        {
            throw new ArgumentNullException("The inventory item to edit cannot be null.");
        }

        try
        {
            var inventoryToEdit = await _context.Inventory.FirstOrDefaultAsync(a => a.Id == inventoryItem.Id) ?? throw new ArgumentNullException("The inventory item to edit cannot be found.");
            inventoryToEdit.Created = vm.BuyDate;

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating inventory item with ID {inventoryItem.Id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteInventoryItem(InventoryItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item), "The FloatItem to remove cannot be null.");
        }

        try
        {
            var account = await _context.CsAccounts.FirstOrDefaultAsync();

            if (account == null)
            {
                return;
            }

            var inventoryItem = await _context.Inventory.FirstOrDefaultAsync(f => f.Id == item.Id);
            if (inventoryItem != null)
            {
                _context.Inventory.Remove(inventoryItem);

                account.Balance += inventoryItem.Price;
                account.PurchasedCount--;

                await _context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine($"FloatItem with ID {item.Id} not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing FloatItem with ID {item.Id}: {ex.Message}");
            throw;
        }
    }

    public async Task DeleteTransactionItem(TransactionItem item)
    {
        if (item == null)
        {
            throw new ArgumentNullException(nameof(item), "The FloatItem to remove cannot be null.");
        }

        try
        {
            var account = await _context.CsAccounts.FirstOrDefaultAsync();

            if (account == null)
            {
                return;
            }

            var transactionItem = await _context.TransactionHistory.FirstOrDefaultAsync(f => f.Id == item.Id);
            if (transactionItem != null)
            {
                _context.TransactionHistory.Remove(transactionItem);

                account.Balance -= transactionItem.PriceAfterTax - transactionItem.BuyPrice;
                account.Profit -= transactionItem.Profit;
                account.PurchasedCount--;
                account.SoldCount--;

                await _context.SaveChangesAsync();
            }
            else
            {
                Console.WriteLine($"FloatItem with ID {item.Id} not found.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing FloatItem with ID {item.Id}: {ex.Message}");
            throw;
        }
    }
}
