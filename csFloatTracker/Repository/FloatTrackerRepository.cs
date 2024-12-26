using csFloatTracker.Context;
using csFloatTracker.Model;
using csFloatTracker.Utils;
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

    public async Task<Result<bool>> BuyFloatAsync(InventoryItem item)
    {
        try
        {
            var account = await _context.CsAccounts.FirstOrDefaultAsync();

            if (account == null)
            {
                return Result<bool>.Fail("Account not found");
            }

            if (account.Balance < item.Price)
            {
                return Result<bool>.Fail("Insufficient funds. Please add funds to your balance.");
            }

            account.Inventory.Add(item);
            account.Balance -= item.Price;
            account.PurchasedCount++;

            await _context.SaveChangesAsync();

            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public async Task<Result<CsAccount>> GetAccountAsync()
    {
        try
        {
            var account = await _context.CsAccounts
                .Include(c => c.Inventory)
                .Include(c => c.TransactionHistory)
                .FirstOrDefaultAsync();

            if (account == null)
            {
                return Result<CsAccount>.Fail("Account not found");
            }

            return Result<CsAccount>.Ok(account);
        }
        catch (Exception ex)
        {
            return Result<CsAccount>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> SellFloatAsync(InventoryItem item, SetSellPriceWindowVM vm)
    {
        if (item == null)
        {
            return Result<bool>.Fail("The inventory item to remove cannot be null");
        }

        try
        {
            var account = await _context.CsAccounts.FirstOrDefaultAsync();

            if (account == null)
            {
                return Result<bool>.Fail("Account not found");
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
                return Result<bool>.Ok(true);
            }
            else
            {
                return Result<bool>.Fail($"Inventory item {item.Name} not found.");
            }
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> UpdateAccountAsync(CsAccount account, EditAccountWindowVM vm)
    {
        if (account == null)
        {
            return Result<bool>.Fail("The Account to edit cannot be null.");
        }

        try
        {
            var accountToEdit = await _context.CsAccounts.FirstOrDefaultAsync(a => a.Id == account.Id);

            if (accountToEdit == null)
            {
                return Result<bool>.Fail("The Account to edit cannot be found.");
            }

            accountToEdit.SoldCount = vm.SoldCount;
            accountToEdit.PurchasedCount = vm.PurchasedCount;
            accountToEdit.Balance = vm.Balance;
            accountToEdit.Profit = vm.Profit;
            accountToEdit.Tax = vm.Tax;

            await _context.SaveChangesAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> UpdateTransactionAsync(TransactionItem transactionItem, EditTransactionWindowVM vm)
    {
        if (transactionItem == null || vm == null)
        {
            return Result<bool>.Fail("The transaction item to edit cannot be null.");
        }

        try
        {
            var transactionToEdit = await _context.TransactionHistory.FirstOrDefaultAsync(a => a.Id == transactionItem.Id);

            if (transactionToEdit == null)
            {
                return Result<bool>.Fail("The transaction item to edit cannot be found.");
            }

            transactionToEdit.SoldDate = vm.SellDate;
            transactionToEdit.CreatedDate = vm.BuyDate;
            transactionToEdit.Float = vm.Float;
            transactionToEdit.Name = vm.Name;

            var hasAccountChanges = (transactionToEdit.BuyPrice != vm.BuyPrice ||
                transactionToEdit.SoldPrice != vm.SellPrice ||
                transactionToEdit.Tax != vm.Tax);

            if (hasAccountChanges)
            {
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
            }

            await _context.SaveChangesAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> UpdateInventoryItemAsync(InventoryItem inventoryItem, EditFloatItemWindowVM vm)
    {
        if (inventoryItem == null)
        {
            return Result<bool>.Fail("The inventory item to edit cannot be null.");
        }

        try
        {
            var inventoryToEdit = await _context.Inventory.FirstOrDefaultAsync(a => a.Id == inventoryItem.Id);

            if (inventoryToEdit == null)
            {
                return Result<bool>.Fail("The inventory item to edit cannot be found.");
            }

            if (inventoryToEdit.Price != vm.Price)
            {
                var account = await _context.CsAccounts.FirstOrDefaultAsync(a => a.Id == inventoryItem.CsAccountId);

                if (account == null)
                {
                    return Result<bool>.Fail("The Account to edit cannot be found.");
                }

                var diff = inventoryToEdit.Price - vm.Price;
                account.Balance += diff;
            }

            inventoryToEdit.Created = vm.BuyDate;
            inventoryToEdit.Price = vm.Price;
            inventoryItem.Float = vm.Float;
            inventoryItem.Name = vm.Name;

            await _context.SaveChangesAsync();
            return Result<bool>.Ok(true);
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteInventoryItem(InventoryItem item)
    {
        if (item == null)
        {
            return Result<bool>.Fail("The inventory item to remove cannot be null.");
        }

        try
        {
            var account = await _context.CsAccounts.FirstOrDefaultAsync();

            if (account == null)
            {
                return Result<bool>.Fail("The Account to edit cannot be found.");
            }

            var inventoryItem = await _context.Inventory.FirstOrDefaultAsync(f => f.Id == item.Id);
            if (inventoryItem != null)
            {
                _context.Inventory.Remove(inventoryItem);

                account.Balance += inventoryItem.Price;
                account.PurchasedCount--;

                await _context.SaveChangesAsync();
                return Result<bool>.Ok(true);
            }
            else
            {
                return Result<bool>.Fail($"Inventory item with {item.Name} not found.");
            }
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }

    public async Task<Result<bool>> DeleteTransactionItem(TransactionItem item)
    {
        if (item == null)
        {
            return Result<bool>.Fail("The transaction item to remove cannot be null.");
        }

        try
        {
            var account = await _context.CsAccounts.FirstOrDefaultAsync();

            if (account == null)
            {
                return Result<bool>.Fail("The Account to edit cannot be found.");
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
                return Result<bool>.Ok(true);
            }
            else
            {
                return Result<bool>.Fail($"Transaction item with {item.Name} not found.");
            }
        }
        catch (Exception ex)
        {
            return Result<bool>.Fail(ex.Message);
        }
    }
}
