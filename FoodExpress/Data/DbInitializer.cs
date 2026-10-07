using FoodExpress.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodExpress.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<FoodExpressDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created and up to date using EF Core migrations
            await context.Database.MigrateAsync();

            // 1. Seed Roles
            string[] roleNames = { "Admin", "RestaurantOwner", "Customer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Seed Admin User
            var adminEmail = "admin@foodexpress.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    PhoneNumber = "+91 98765 43210",
                    EmailConfirmed = true,
                    Address = "100 Connaught Place",
                    City = "New Delhi",
                    State = "Delhi",
                    PostalCode = "110001",
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Restaurant Owner User
            var ownerEmail = "owner@foodexpress.com";
            var ownerUser = await userManager.FindByEmailAsync(ownerEmail);
            if (ownerUser == null)
            {
                ownerUser = new ApplicationUser
                {
                    UserName = ownerEmail,
                    Email = ownerEmail,
                    FullName = "Ramesh Patel (Chef & Owner)",
                    PhoneNumber = "+91 98250 12345",
                    EmailConfirmed = true,
                    Address = "45 MG Road",
                    City = "Bengaluru",
                    State = "Karnataka",
                    PostalCode = "560001",
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(ownerUser, "Owner@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(ownerUser, "RestaurantOwner");
                }
            }

            // 4. Seed Customer User
            var customerEmail = "customer@foodexpress.com";
            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            if (customerUser == null)
            {
                customerUser = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    FullName = "Rahul Sharma",
                    PhoneNumber = "+91 98111 22334",
                    EmailConfirmed = true,
                    Address = "Flat 402, Green Valley Apartments, Sector 18",
                    City = "Noida",
                    State = "Uttar Pradesh",
                    PostalCode = "201301",
                    CreatedDate = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(customerUser, "Customer@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");
                }
            }

            // 5. Seed Food Categories
            if (!context.Categories.Any())
            {
                var categories = new List<FoodCategory>
                {
                    new FoodCategory { Name = "Burgers & Sandwiches", Description = "Juicy flame-grilled gourmet burgers, brioche buns, and crispy fries", Image = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&auto=format&fit=crop&q=80", IsActive = true },
                    new FoodCategory { Name = "Artisan Pizzas", Description = "Hand-tossed Italian crusts baked to bubbly woodfired perfection", Image = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=600&auto=format&fit=crop&q=80", IsActive = true },
                    new FoodCategory { Name = "Asian & Noodles", Description = "Fragrant stir-fries, ramen bowls, dim sum, and sushi delicacies", Image = "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=600&auto=format&fit=crop&q=80", IsActive = true },
                    new FoodCategory { Name = "Healthy Salads & Bowls", Description = "Farm-fresh vibrant greens, grain bowls, and nutrient-dense vitality meals", Image = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=600&auto=format&fit=crop&q=80", IsActive = true },
                    new FoodCategory { Name = "Desserts & Bakery", Description = "Decadent chocolate lava cakes, artisan gelato, and sweet pastries", Image = "https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=600&auto=format&fit=crop&q=80", IsActive = true },
                    new FoodCategory { Name = "Beverages & Shakes", Description = "Freshly squeezed juices, cold craft brews, and thick milkshakes", Image = "https://images.unsplash.com/photo-1551024709-8f23befc6f87?w=600&auto=format&fit=crop&q=80", IsActive = true }
                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // 6. Seed Restaurants
            if (!context.Restaurants.Any() && ownerUser != null)
            {
                var restaurants = new List<Restaurant>
                {
                    new Restaurant
                    {
                        Name = "Burger Republic & Grill",
                        Description = "Specialty handcrafted gourmet burgers, crispy peri peri fries, and thick chocolate shakes.",
                        Address = "120 MG Road",
                        City = "Mumbai",
                        Phone = "+91 98222 33445",
                        Email = "info@burgerrepublic.com",
                        Image = "https://images.unsplash.com/photo-1555396273-367ea4eb4db5?w=700&auto=format&fit=crop&q=80",
                        IsActive = true,
                        OpeningTime = new TimeSpan(10, 0, 0),
                        ClosingTime = new TimeSpan(23, 0, 0),
                        OwnerId = ownerUser.Id,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Restaurant
                    {
                        Name = "Bella Napoli Woodfired Pizza",
                        Description = "Authentic Neapolitan sourdough pizzas, rich burrata salads, and Italian desserts.",
                        Address = "340 Park Street",
                        City = "Kolkata",
                        Phone = "+91 98333 44556",
                        Email = "orders@bellanapoli.com",
                        Image = "https://images.unsplash.com/photo-1517248135467-4c7edcad34c4?w=700&auto=format&fit=crop&q=80",
                        IsActive = true,
                        OpeningTime = new TimeSpan(11, 0, 0),
                        ClosingTime = new TimeSpan(22, 30, 0),
                        OwnerId = ownerUser.Id,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Restaurant
                    {
                        Name = "Tokyo Fusion & Sushi Bar",
                        Description = "Sashimi grade sushi rolls, tonkotsu ramen, gyoza, and bento specialties.",
                        Address = "55 Brigade Road",
                        City = "Bengaluru",
                        Phone = "+91 98444 55667",
                        Email = "contact@tokyofusion.com",
                        Image = "https://images.unsplash.com/photo-1552611052-33e04de081de?w=700&auto=format&fit=crop&q=80",
                        IsActive = true,
                        OpeningTime = new TimeSpan(11, 30, 0),
                        ClosingTime = new TimeSpan(22, 0, 0),
                        OwnerId = ownerUser.Id,
                        CreatedDate = DateTime.UtcNow
                    },
                    new Restaurant
                    {
                        Name = "Green Leaf Organic Kitchen",
                        Description = "100% organic macro bowls, antioxidant smoothies, vegan wraps, and fresh salads.",
                        Address = "88 Banjara Hills, Road No. 12",
                        City = "Hyderabad",
                        Phone = "+91 98555 66778",
                        Email = "hello@greenleafkitchen.com",
                        Image = "https://images.unsplash.com/photo-1540420773420-3366772f4999?w=700&auto=format&fit=crop&q=80",
                        IsActive = true,
                        OpeningTime = new TimeSpan(8, 30, 0),
                        ClosingTime = new TimeSpan(20, 0, 0),
                        OwnerId = ownerUser.Id,
                        CreatedDate = DateTime.UtcNow
                    }
                };
                context.Restaurants.AddRange(restaurants);
                await context.SaveChangesAsync();
            }

            // 6b. Ensure all existing restaurant phone numbers conform to Indian mobile format
            var existingRestaurants = await context.Restaurants.ToListAsync();
            bool hasPhoneUpdates = false;
            foreach (var rest in existingRestaurants)
            {
                var formatted = Restaurant.FormatIndianPhoneNumber(rest.Phone);
                if (!string.IsNullOrEmpty(formatted) && rest.Phone != formatted)
                {
                    rest.Phone = formatted;
                    hasPhoneUpdates = true;
                }
            }
            if (hasPhoneUpdates)
            {
                await context.SaveChangesAsync();
            }

            // 7. Seed Food Items
            if (!context.FoodItems.Any())
            {
                var burgerRest = await context.Restaurants.FirstOrDefaultAsync(r => r.Name.Contains("Burger"));
                var pizzaRest = await context.Restaurants.FirstOrDefaultAsync(r => r.Name.Contains("Napoli"));
                var tokyoRest = await context.Restaurants.FirstOrDefaultAsync(r => r.Name.Contains("Tokyo"));
                var greenRest = await context.Restaurants.FirstOrDefaultAsync(r => r.Name.Contains("Green"));

                var burgerCat = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Burger"));
                var pizzaCat = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Pizza"));
                var asianCat = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Asian"));
                var saladCat = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Salad"));
                var dessertCat = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Dessert"));
                var beverageCat = await context.Categories.FirstOrDefaultAsync(c => c.Name.Contains("Beverage"));

                var items = new List<FoodItem>();

                if (burgerRest != null && burgerCat != null)
                {
                    items.Add(new FoodItem { Name = "The Big Boss Angus Burger", Description = "Double prime Angus beef, cheddar melt, caramelized onions, smoked bacon & truffle aioli.", Price = 14.99m, Image = "https://images.unsplash.com/photo-1568901346375-23c9450c58cd?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = burgerCat.Id, RestaurantId = burgerRest.Id });
                    items.Add(new FoodItem { Name = "Crispy Nashville Hot Chicken", Description = "Spicy buttermilk fried chicken breast, pickles, chipotle slaw on toasted brioche.", Price = 12.50m, Image = "https://images.unsplash.com/photo-1625813506062-0aeb1d7a094b?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = burgerCat.Id, RestaurantId = burgerRest.Id });
                    items.Add(new FoodItem { Name = "Parmesan Truffle Fries", Description = "Skin-on golden potato fries tossed in aromatic white truffle oil, sea salt and freshly grated aged parmesan.", Price = 6.99m, Image = "https://images.unsplash.com/photo-1573080496219-bb080dd4f877?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = burgerCat.Id, RestaurantId = burgerRest.Id });
                    if (beverageCat != null)
                    {
                        items.Add(new FoodItem { Name = "Classic Salted Caramel Shake", Description = "Hand-spun bourbon vanilla bean ice cream with ribbon swirls of salted butter caramel.", Price = 5.99m, Image = "https://images.unsplash.com/photo-1572490122747-3968b75cc699?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = beverageCat.Id, RestaurantId = burgerRest.Id });
                    }
                }

                if (pizzaRest != null && pizzaCat != null)
                {
                    items.Add(new FoodItem { Name = "Margherita D.O.P.", Description = "San Marzano tomatoes, fresh buffalo mozzarella, fresh sweet basil leaves & extra virgin olive oil.", Price = 16.50m, Image = "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = pizzaCat.Id, RestaurantId = pizzaRest.Id });
                    items.Add(new FoodItem { Name = "Diavola Spicy Pepperoni", Description = "Spicy Calabrian salami, smoked provolone, crushed red pepper flakes and wildflower hot honey drizzle.", Price = 18.99m, Image = "https://images.unsplash.com/photo-1534308983496-4fabb1a015ee?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = pizzaCat.Id, RestaurantId = pizzaRest.Id });
                    items.Add(new FoodItem { Name = "Tartufo Wild Mushroom", Description = "Roasted cremini, shiitake, fontina cheese, garlic crema, and fresh thyme.", Price = 19.50m, Image = "https://images.unsplash.com/photo-1513104890138-7c749659a591?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = pizzaCat.Id, RestaurantId = pizzaRest.Id });
                    if (dessertCat != null)
                    {
                        items.Add(new FoodItem { Name = "Traditional Tiramisu", Description = "Espresso-soaked savoiardi ladyfingers layered with rich mascarpone zabaglione and Dutch cocoa powder.", Price = 7.50m, Image = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = dessertCat.Id, RestaurantId = pizzaRest.Id });
                    }
                }

                if (tokyoRest != null && asianCat != null)
                {
                    items.Add(new FoodItem { Name = "Dragon Roll Platter", Description = "Tempura shrimp, cucumber, avocado slices, barbecue unagi eel sauce and toasted sesame seeds.", Price = 17.95m, Image = "https://images.unsplash.com/photo-1579871494447-9811cf80d66c?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = asianCat.Id, RestaurantId = tokyoRest.Id });
                    items.Add(new FoodItem { Name = "Signature Tonkotsu Ramen", Description = "Rich 24-hour pork bone broth, springy noodles, chashu pork belly, ajitsuke tamago egg & nori.", Price = 15.75m, Image = "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = asianCat.Id, RestaurantId = tokyoRest.Id });
                    items.Add(new FoodItem { Name = "Crispy Pan-Fried Pork Gyoza", Description = "Six handmade dumplings filled with ground pork, scallions, ginger, served with citrus ponzu sauce.", Price = 8.25m, Image = "https://images.unsplash.com/photo-1496116218417-1a781b1c416c?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = asianCat.Id, RestaurantId = tokyoRest.Id });
                }

                if (greenRest != null && saladCat != null)
                {
                    items.Add(new FoodItem { Name = "Mediterranean Quinoa Bowl", Description = "Organic tri-color quinoa, kalamata olives, heirloom cherry tomatoes, English cucumbers, whipped feta, tahini lemon dressing.", Price = 13.99m, Image = "https://images.unsplash.com/photo-1540420773420-3366772f4999?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = saladCat.Id, RestaurantId = greenRest.Id });
                    items.Add(new FoodItem { Name = "Avocado Green Goddess Bowl", Description = "Hass avocado, baby spinach, edamame, roasted chickpeas, broccoli florets and herb green goddess vinaigrette.", Price = 14.50m, Image = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = saladCat.Id, RestaurantId = greenRest.Id });
                    if (beverageCat != null)
                    {
                        items.Add(new FoodItem { Name = "Cold-Pressed Super Detox Juice", Description = "Organic kale, green apple, cucumber, celery, ginger, and fresh Meyer lemon.", Price = 6.50m, Image = "https://images.unsplash.com/photo-1613478223719-2ab802602423?w=600&auto=format&fit=crop&q=80", IsAvailable = true, CategoryId = beverageCat.Id, RestaurantId = greenRest.Id });
                    }
                }

                context.FoodItems.AddRange(items);
                await context.SaveChangesAsync();
            }

            // 8. Seed Coupons
            if (!context.Coupons.Any())
            {
                var coupons = new List<Coupon>
                {
                    new Coupon
                    {
                        Code = "WELCOME20",
                        Description = "Get 20% off your order! Perfect welcome gift for new FoodExpress foodies.",
                        DiscountType = DiscountType.Percentage,
                        DiscountValue = 20.00m,
                        MinimumOrderAmount = 15.00m,
                        StartDate = DateTime.UtcNow.Date.AddDays(-10),
                        EndDate = DateTime.UtcNow.Date.AddYears(1),
                        IsActive = true
                    },
                    new Coupon
                    {
                        Code = "SAVE10",
                        Description = "Flat $10 discount on orders exceeding $30.",
                        DiscountType = DiscountType.FixedAmount,
                        DiscountValue = 10.00m,
                        MinimumOrderAmount = 30.00m,
                        StartDate = DateTime.UtcNow.Date.AddDays(-10),
                        EndDate = DateTime.UtcNow.Date.AddYears(1),
                        IsActive = true
                    }
                };
                context.Coupons.AddRange(coupons);
                await context.SaveChangesAsync();
            }

            // 9. Seed Addresses
            if (!context.Addresses.Any() && customerUser != null)
            {
                var addresses = new List<Address>
                {
                    new Address
                    {
                        UserId = customerUser.Id,
                        Title = "Home",
                        AddressLine = "Flat 402, Green Valley Apartments, Sector 18",
                        City = "Noida",
                        State = "Uttar Pradesh",
                        PostalCode = "201301",
                        PhoneNumber = "+91 98111 22334",
                        IsDefault = true
                    },
                    new Address
                    {
                        UserId = customerUser.Id,
                        Title = "Office",
                        AddressLine = "Cyber City, DLF Phase 2, Tower B",
                        City = "Gurugram",
                        State = "Haryana",
                        PostalCode = "122002",
                        PhoneNumber = "+91 98111 22334",
                        IsDefault = false
                    }
                };
                context.Addresses.AddRange(addresses);
                await context.SaveChangesAsync();
            }

            // 10. Seed Reviews
            if (!context.Reviews.Any() && customerUser != null)
            {
                var burgerRest = await context.Restaurants.FirstOrDefaultAsync(r => r.Name.Contains("Burger"));
                var pizzaRest = await context.Restaurants.FirstOrDefaultAsync(r => r.Name.Contains("Napoli"));

                var reviews = new List<Review>();
                if (burgerRest != null)
                {
                    reviews.Add(new Review
                    {
                        UserId = customerUser.Id,
                        RestaurantId = burgerRest.Id,
                        Rating = 5,
                        Comment = "Outstanding burger quality! The brioche bun was warm and soft, and the patty was perfectly juicy. Delivery arrived in 25 minutes.",
                        CreatedDate = DateTime.UtcNow.AddDays(-3),
                        IsApproved = true
                    });
                }
                if (pizzaRest != null)
                {
                    reviews.Add(new Review
                    {
                        UserId = customerUser.Id,
                        RestaurantId = pizzaRest.Id,
                        Rating = 5,
                        Comment = "Best Neapolitan pizza in the city! Crust was blistered to perfection and the spicy peri peri dip was unbelievable.",
                        CreatedDate = DateTime.UtcNow.AddDays(-1),
                        IsApproved = true
                    });
                }
                context.Reviews.AddRange(reviews);
                await context.SaveChangesAsync();
            }

            // 11. Seed Sample Order
            if (!context.Orders.Any() && customerUser != null)
            {
                var burgerRest = await context.Restaurants.FirstOrDefaultAsync(r => r.Name.Contains("Burger"));
                var burgerFood = await context.FoodItems.FirstOrDefaultAsync(f => f.RestaurantId == (burgerRest != null ? burgerRest.Id : 0));

                if (burgerRest != null && burgerFood != null)
                {
                    var sampleOrder = new Order
                    {
                        UserId = customerUser.Id,
                        RestaurantId = burgerRest.Id,
                        OrderDate = DateTime.UtcNow.AddHours(-2),
                        SubTotal = burgerFood.Price * 2,
                        DiscountAmount = 0,
                        DeliveryFee = 49.00m,
                        TotalAmount = (burgerFood.Price * 2) + 49.00m,
                        DeliveryAddress = "Flat 402, Green Valley Apartments, Sector 18, Noida, Uttar Pradesh 201301",
                        PhoneNumber = "+91 98111 22334",
                        PaymentMethod = PaymentMethod.CashOnDelivery,
                        PaymentStatus = PaymentStatus.Pending,
                        OrderStatus = OrderStatus.Preparing,
                        Notes = "Please include extra napkins and mint chutney."
                    };
                    sampleOrder.OrderItems.Add(new OrderItem
                    {
                        FoodItemId = burgerFood.Id,
                        Quantity = 2,
                        Price = burgerFood.Price,
                        SubTotal = burgerFood.Price * 2
                    });
                    context.Orders.Add(sampleOrder);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
