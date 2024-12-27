using csFloatTracker.Context;
using csFloatTracker.Model;
using csFloatTracker.Utils;
using csFloatTracker.ViewModel.InternalWindows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;

namespace csFloatTracker.Repository;

public class FloatTrackerRepository
{
    private readonly FloatTrackerContext _context;

    public FloatTrackerRepository(FloatTrackerContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> BuyFloatAsync(InventoryItem item)
    {
        try
        {
            App.Logger.LogInformation("Attempting to purchase inventory item. Name: {Name}, ID: {ID}", item.Name, item.Id);

            var account = await _context.CsAccounts.FirstOrDefaultAsync();

            if (account == null)
            {
                App.Logger.LogWarning("Account not found while trying to purchase inventory item. Name: {Name}, ID: {ID}", item.Name, item.Id);
                return Result<bool>.Fail("Account not found");
            }

            if (account.Balance < item.Price)
            {
                App.Logger.LogWarning("Insufficient funds for purchase. Account balance: {Balance}, Item price: {Price}", account.Balance, item.Price);
                return Result<bool>.Fail("Insufficient funds. Please add funds to your balance.");
            }

            account.Inventory.Add(item);
            account.Balance -= item.Price;
            account.PurchasedCount++;

            await _context.SaveChangesAsync();

            App.Logger.LogInformation("Purchase successful. Updated account with ID: {ID}", account.Id);

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            App.Logger.LogError(ex, "Error occurred during inventory item purchase. Name: {Name}, ID: {ID}", item.Name, item.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }

    public async Task<Result<CsAccount>> GetAccountAsync()
    {
        try
        {
            App.Logger.LogInformation("Attempting to retrieve account.");

            var account = await _context.CsAccounts
                .Include(c => c.Inventory)
                .Include(c => c.TransactionHistory)
                .FirstOrDefaultAsync();

            if (account == null)
            {
                App.Logger.LogWarning("Account not found.");
                return Result<CsAccount>.Fail("Account not found");
            }

            App.Logger.LogInformation("Account retrieved successfully. ID: {ID}", account.Id);
            return Result<CsAccount>.Ok(account);
        }
        catch (Exception ex)
        {
            App.Logger.LogError(ex, "An error occurred while retrieving the account.");
            return Result<CsAccount>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> SellFloatAsync(InventoryItem item, SetSellPriceWindowVM vm)
    {
        if (item == null)
        {
            App.Logger.LogWarning("The inventory item to remove cannot be null.");
            return Result<bool>.Fail("The inventory item to remove cannot be null.");
        }

        try
        {
            App.Logger.LogInformation("Starting the process to sell an inventory item. Name: {Name}, ID: {ID}", item.Name, item.Id);

            var account = await _context.CsAccounts.FirstOrDefaultAsync();
            if (account == null)
            {
                App.Logger.LogWarning("Account not found.");
                return Result<bool>.Fail("Account not found.");
            }

            var inventoryItem = await _context.Inventory.FirstOrDefaultAsync(f => f.Id == item.Id);
            if (inventoryItem != null)
            {
                App.Logger.LogInformation("Inventory item found. Name: {Name}, ID: {ID}", inventoryItem.Name, inventoryItem.Id);

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

                App.Logger.LogInformation("Creating a transaction item. Name: {Name}, ID: {ID}", transactionItem.Name, transactionItem.Id);

                _context.TransactionHistory.Add(transactionItem);
                _context.Inventory.Remove(inventoryItem);

                account.Balance += sellPriceAfterTax;
                account.SoldCount++;
                account.Profit += profit;

                await _context.SaveChangesAsync();

                App.Logger.LogInformation("Item sold successfully. Account updated. ID: {ID}", account.Id);
                return Result<bool>.Ok(true);
            }
            else
            {
                App.Logger.LogWarning("Inventory item not found. Name: {Name}, ID: {ID}", item.Name, item.Id);
                return Result<bool>.Fail($"Inventory item {item.Name} not found.");
            }
        }
        catch (Exception ex)
        {
            App.Logger.LogError(ex, "An error occurred while selling the inventory item. Name: {Name}, ID: {ID}", item.Name, item.Id);
            return Result<bool>.Fail("An unexpected error occurred while processing the request.");
        }
    }

    public async Task<Result<bool>> UpdateAccountAsync(CsAccount account, EditAccountWindowVM vm)
    {
        if (account == null)
        {
            App.Logger.LogWarning("The Account to edit cannot be null.");
            return Result<bool>.Fail("The Account to edit cannot be null.");
        }

        try
        {
            App.Logger.LogInformation("Starting the process to update account. ID: {ID}", account.Id);

            var accountToEdit = await _context.CsAccounts.FirstOrDefaultAsync(a => a.Id == account.Id);

            if (accountToEdit == null)
            {
                App.Logger.LogWarning("The Account to edit cannot be found. ID: {ID}", account.Id);
                return Result<bool>.Fail("The Account to edit cannot be found.");
            }

            App.Logger.LogInformation("Account found. Updating fields: SoldCount={SoldCount}, PurchasedCount={PurchasedCount}, Balance={Balance}, Profit={Profit}, Tax={Tax}",
                vm.SoldCount, vm.PurchasedCount, vm.Balance, vm.Profit, vm.Tax);

            accountToEdit.SoldCount = vm.SoldCount;
            accountToEdit.PurchasedCount = vm.PurchasedCount;
            accountToEdit.Balance = vm.Balance;
            accountToEdit.Profit = vm.Profit;
            accountToEdit.Tax = vm.Tax;

            await _context.SaveChangesAsync();

            App.Logger.LogInformation("Account updated successfully. ID: {ID}", accountToEdit.Id);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            App.Logger.LogError(ex, "An error occurred while updating account. ID: {ID}", account.Id);
            return Result<bool>.Fail("An unexpected error occurred while processing the request.");
        }
    }

    public async Task<Result<bool>> UpdateTransactionAsync(TransactionItem transactionItem, EditTransactionWindowVM vm)
    {
        if (transactionItem == null || vm == null)
        {
            App.Logger.LogWarning("The transaction item or view model to edit cannot be null.");
            return Result<bool>.Fail("The transaction item to edit cannot be null.");
        }
        try
        {
            App.Logger.LogInformation("Starting the process to update transaction. Name: {Name}, ID: {ID}", transactionItem.Name, transactionItem.Id);

            var transactionToEdit = await _context.TransactionHistory.FirstOrDefaultAsync(a => a.Id == transactionItem.Id);

            if (transactionToEdit == null)
            {
                App.Logger.LogWarning("The transaction item to edit cannot be found. Name: {Name}, ID: {ID}", transactionItem.Name, transactionItem.Id);
                return Result<bool>.Fail("The transaction item to edit cannot be found.");
            }

            App.Logger.LogInformation("Transaction found. Updating fields: SellDate={SellDate}, BuyDate={BuyDate}, Float={Float}, Name={Name}",
                vm.SellDate, vm.BuyDate, vm.Float, vm.Name);

            transactionToEdit.SoldDate = vm.SellDate;
            transactionToEdit.CreatedDate = vm.BuyDate;
            transactionToEdit.Float = vm.Float;
            transactionToEdit.Name = vm.Name;

            var hasAccountChanges = (transactionToEdit.BuyPrice != vm.BuyPrice ||
                                     transactionToEdit.SoldPrice != vm.SellPrice ||
                                     transactionToEdit.Tax != vm.Tax);

            if (hasAccountChanges)
            {
                App.Logger.LogInformation("Account changes detected. Previous Profit: {PrevProfit}, New BuyPrice={BuyPrice}, New SoldPrice={SellPrice}, New Tax={Tax}",
                    transactionToEdit.Profit, vm.BuyPrice, vm.SellPrice, vm.Tax);

                var prevProfit = transactionToEdit.Profit;

                transactionToEdit.BuyPrice = vm.BuyPrice;
                transactionToEdit.SoldPrice = vm.SellPrice;
                transactionToEdit.Tax = vm.Tax;

                var account = await _context.CsAccounts.FirstOrDefaultAsync(a => a.Id == transactionToEdit.CsAccountId) ?? throw new ArgumentNullException("The account to edit cannot be found.");

                transactionToEdit.PriceAfterTax = transactionToEdit.SoldPrice - (transactionToEdit.SoldPrice * transactionToEdit.Tax / 100);
                transactionToEdit.Profit = transactionToEdit.PriceAfterTax - transactionToEdit.BuyPrice;

                var diff = transactionToEdit.Profit - prevProfit;
                account.Balance += diff;
                account.Profit += diff;

                App.Logger.LogInformation("Account updated. Profit diff: {Diff}, New Balance={Balance}, New Profit={Profit}",
                    diff, account.Balance, account.Profit);
            }

            await _context.SaveChangesAsync();

            App.Logger.LogInformation("Transaction updated successfully. Name: {Name}, ID: {ID}", transactionItem.Name, transactionItem.Id);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            App.Logger.LogError(ex, "An error occurred while updating transaction. Name: {Name}, ID: {ID}", transactionItem.Name, transactionItem.Id);
            return Result<bool>.Fail("An unexpected error occurred while processing the request.");
        }
    }

    public async Task<Result<bool>> UpdateInventoryItemAsync(InventoryItem inventoryItem, EditFloatItemWindowVM vm)
    {
        if (inventoryItem == null)
        {
            App.Logger.LogWarning("The inventory item to edit cannot be null.");
            return Result<bool>.Fail("The inventory item to edit cannot be null.");
        }

        try
        {
            App.Logger.LogInformation("Starting the process to update inventory item. Name: {Name}, ID: {ID}", inventoryItem.Name, inventoryItem.Id);

            var inventoryToEdit = await _context.Inventory.FirstOrDefaultAsync(a => a.Id == inventoryItem.Id);

            if (inventoryToEdit == null)
            {
                App.Logger.LogWarning("The inventory item to edit cannot be found. Name: {Name}, ID: {ID}", inventoryItem.Name, inventoryItem.Id);
                return Result<bool>.Fail("The inventory item to edit cannot be found.");
            }

            App.Logger.LogInformation("Inventory item found. Updating fields: Price={Price}, Float={Float}, Name={Name}",
                vm.Price, vm.Float, vm.Name);

            if (inventoryToEdit.Price != vm.Price)
            {
                var account = await _context.CsAccounts.FirstOrDefaultAsync(a => a.Id == inventoryItem.CsAccountId);

                if (account == null)
                {
                    App.Logger.LogWarning("The account associated with the inventory item cannot be found. Name: {Name}, ID: {ID}", inventoryItem.Name, inventoryItem.Id);
                    return Result<bool>.Fail("The Account to edit cannot be found.");
                }

                var diff = inventoryToEdit.Price - vm.Price;
                account.Balance += diff;

                App.Logger.LogInformation("Account balance updated. Price diff: {Diff}, New Balance: {Balance}",
                    diff, account.Balance);
            }

            inventoryToEdit.Created = vm.BuyDate;
            inventoryToEdit.Price = vm.Price;
            inventoryToEdit.Float = vm.Float;
            inventoryToEdit.Name = vm.Name;

            await _context.SaveChangesAsync();

            App.Logger.LogInformation("Inventory item updated successfully. Name: {Name}, ID: {ID}", inventoryToEdit.Name, inventoryToEdit.Id);
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            App.Logger.LogError(ex, "An error occurred while updating inventory item. Name: {Name}, ID: {ID}", inventoryItem.Name, inventoryItem.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteInventoryItem(InventoryItem item)
    {
        if (item == null)
        {
            App.Logger.LogWarning("The inventory item to remove cannot be null.");
            return Result<bool>.Fail("The inventory item to remove cannot be null.");
        }

        try
        {
            App.Logger.LogInformation("Starting the process to delete inventory item. Name: {Name}, ID: {ID}", item.Name, item.Id);

            var account = await _context.CsAccounts.FirstOrDefaultAsync();

            if (account == null)
            {
                App.Logger.LogWarning("The account to edit cannot be found.");
                return Result<bool>.Fail("The Account to edit cannot be found.");
            }

            var inventoryItem = await _context.Inventory.FirstOrDefaultAsync(f => f.Id == item.Id);
            if (inventoryItem != null)
            {
                App.Logger.LogInformation("Inventory item found. Removing item. Name: {Name}, ID: {ID}", inventoryItem.Name, inventoryItem.Id);

                _context.Inventory.Remove(inventoryItem);

                account.Balance += inventoryItem.Price;
                account.PurchasedCount--;

                App.Logger.LogInformation("Account balance updated. Item price: {Price}, New Balance: {Balance}", inventoryItem.Price, account.Balance);
                App.Logger.LogInformation("Purchased count updated. New Purchased Count: {PurchasedCount}", account.PurchasedCount);

                await _context.SaveChangesAsync();
                return Result<bool>.Ok(true);
            }
            else
            {
                App.Logger.LogWarning("Inventory item not found. Name: {Name}, ID: {ID}", item.Name, item.Id);
                return Result<bool>.Fail($"Inventory item with {item.Name} not found.");
            }
        }
        catch (Exception ex)
        {
            App.Logger.LogError(ex, "An error occurred while deleting inventory item. Name: {Name}, ID: {ID}", item.Name, item.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteTransactionItem(TransactionItem item)
    {
        if (item == null)
        {
            App.Logger.LogWarning("The transaction item to remove cannot be null.");
            return Result<bool>.Fail("The transaction item to remove cannot be null.");
        }

        try
        {
            App.Logger.LogInformation("Starting the process to delete transaction item. Name: {Name}, ID: {ID}", item.Name, item.Id);

            var account = await _context.CsAccounts.FirstOrDefaultAsync();

            if (account == null)
            {
                App.Logger.LogWarning("The account to edit cannot be found.");
                return Result<bool>.Fail("The Account to edit cannot be found.");
            }

            var transactionItem = await _context.TransactionHistory.FirstOrDefaultAsync(f => f.Id == item.Id);
            if (transactionItem != null)
            {
                App.Logger.LogInformation("Transaction item found. Removing item. Name: {Name}, ID: {ID}", transactionItem.Name, transactionItem.Id);

                _context.TransactionHistory.Remove(transactionItem);

                account.Balance -= transactionItem.PriceAfterTax - transactionItem.BuyPrice;
                account.Profit -= transactionItem.Profit;
                account.PurchasedCount--;
                account.SoldCount--;

                App.Logger.LogInformation("Account balance updated. Price difference: {PriceDiff}, New Balance: {Balance}",
                    transactionItem.PriceAfterTax - transactionItem.BuyPrice, account.Balance);
                App.Logger.LogInformation("Account profit updated. New Profit: {Profit}", account.Profit);
                App.Logger.LogInformation("Purchased count updated. New Purchased Count: {PurchasedCount}", account.PurchasedCount);
                App.Logger.LogInformation("Sold count updated. New Sold Count: {SoldCount}", account.SoldCount);

                await _context.SaveChangesAsync();
                return Result<bool>.Ok(true);
            }
            else
            {
                App.Logger.LogWarning("Transaction item not found. Name: {Name}, ID: {ID}", item.Name, item.Id);
                return Result<bool>.Fail($"Transaction item with {item.Name} not found.");
            }
        }
        catch (Exception ex)
        {
            App.Logger.LogError(ex, "An error occurred while deleting transaction item. Name: {Name}, ID: {ID}", item.Name, item.Id);
            return Result<bool>.Fail(ex.Message);
        }
    }
}
