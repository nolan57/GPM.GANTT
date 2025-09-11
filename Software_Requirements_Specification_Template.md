# Software Requirements Specification Template for Non-Technical Users

## Project Information

**Project Name:** [Enter the name of your project]

**Document Version:** [Enter version number, e.g., 1.0]

**Prepared By:** [Your name or team name]

**Date:** [Enter date]

**Approved By:** [Name of approver, if applicable]

---

## 1. Introduction

### 1.1 Purpose
> Describe in simple terms what this software will do. What problem does it solve for the users?

*Example: This document describes what we want our online store website to do - let customers browse products, add them to a shopping cart, and complete purchases securely.*

### 1.2 Scope
> Who will use this software? Where will it be used? What are the main benefits?

*Example: Our online store will be used by customers shopping from home or work. They'll be able to shop 24/7 from any device with internet, and we'll save time by not needing a physical store.*

### 1.3 Definitions, Acronyms, and Abbreviations
> Explain any technical terms in simple language.

| Term | Definition |
|------|------------|
| User | A person who will use the software |
| Dashboard | The main screen that shows important information at a glance |
| Database | A digital filing cabinet that stores all our information |

### 1.4 References
> List any documents or sources that were used to create this document.

*Example: 
- Our existing paper catalog
- Feedback from customers about our current shopping process
- Examples of competitor websites we like*

### 1.5 Overview
> Briefly describe what each section of this document covers.

*This document explains everything we want our software to do. It starts with what the software is for, then describes who will use it, and finally lists all the specific features we need.*

---

## 2. Overall Description

### 2.1 Product Perspective
> How does this software fit into the bigger picture? Will it work with other systems?

*Example: Our customer website will be separate from our internal systems for now. Customers can use it without needing to log into anything else, but eventually we'd like to connect it to our inventory system.*

### 2.2 Product Functions
> List the main things the software will do in simple terms.

1. [Function 1 - e.g., Show a catalog of all our products]
2. [Function 2 - e.g., Let customers save items to a shopping cart]
3. [Function 3 - e.g., Process payments securely]
4. [Function 4 - e.g., Send order confirmations by email]

### 2.3 User Characteristics
> Who will use this software? What is their technical level?

| User Type | Description | Technical Level |
|-----------|-------------|-----------------|
| Customer | Person buying our products | Basic computer skills |
| Store Manager | Person managing products and orders | Comfortable with business software |

### 2.4 Constraints
> What limitations might affect the software? (Budget, time, technology, etc.)

*Example:
- Must be ready for the holiday season
- Budget of $25,000 for initial version
- Must work on phones, tablets, and computers
- All customer information must be kept private*

### 2.5 Assumptions and Dependencies
> What do you assume to be true? What does this software depend on?

*Example:
- Customers will have credit cards or PayPal accounts
- We'll have reliable internet service
- We can hire a payment processing company*

---

## 3. Specific Requirements

### 3.1 Functional Requirements
> These are the specific features the software must have. Use simple language and include examples.

#### 3.1.1 User Accounts
**Description:** People should be able to create accounts and log in to save their information.

**Features:**
- [ ] People can create accounts with email and password
- [ ] People can log in to their accounts
- [ ] People can reset their password if they forget it
- [ ] The system sends a confirmation email when they sign up

**Example:** When someone wants to buy something, they can create an account with their email address. They'll get an email to confirm their address, then create a password. Next time they visit, they just enter their email and password to access their saved information.

#### 3.1.2 Product Catalog
**Description:** Customers need to browse our products easily.

**Features:**
- [ ] Show all products on categorized pages
- [ ] Let customers search for products by name or description
- [ ] Let customers filter products by category, price, or features
- [ ] Show clear photos and descriptions for each product
- [ ] Show prices clearly, including any discounts

**Example:** A customer visits our "Home & Garden" section and sees all our gardening tools. They can sort by price or search for "pruning shears" to find exactly what they want. Each tool has several photos and a clear description of what it does.

#### 3.1.3 Shopping Cart
**Description:** Customers need to save items while they shop and checkout when ready.

**Features:**
- [ ] Add items to a virtual shopping cart
- [ ] See what's in the cart at any time
- [ ] Change quantities or remove items
- [ ] See the total cost including tax and shipping
- [ ] Save the cart to finish shopping later (for logged-in users)

**Example:** A customer adds a $25 plant and a $15 watering can to their cart. They can see both items listed with the prices, and the total shows $40 plus $5 shipping. If they change their mind about the watering can, they can remove it before checking out.

### 3.2 Non-Functional Requirements
> These describe how well the software should work, not what it should do.

#### 3.2.1 Performance
> How fast should the software be?

- The website should load quickly (under 3 seconds)
- Searching for products should be instant (under 1 second)
- The system should handle many customers shopping at once

#### 3.2.2 Security
> How should the software protect sensitive information?

- Customer passwords should be kept secret and secure
- Customer information should be backed up regularly
- Only authorized people should access order information
- All communication should be encrypted (like putting it in a sealed envelope)

#### 3.2.3 Usability
> How easy should the software be to use?

- First-time visitors should understand how to shop within 2 minutes
- The website should be easy to navigate with clear buttons
- Error messages should explain what went wrong in plain language
- The website should work on phones, tablets, and computers

#### 3.2.4 Reliability
> How consistently should the software work?

- The website should be available for customers to shop 99% of the time
- Customer orders and information should never be lost
- If something goes wrong, the system should fix itself or alert us

#### 3.2.5 Compatibility
> What devices or systems should the software work with?

- Must work on popular web browsers (Chrome, Firefox, Safari, Edge)
- Must work well on mobile phones and tablets
- Should work on Windows, Mac, and Linux computers

---

## 4. External Interface Requirements

### 4.1 User Interfaces
> Describe what the screens should look like in general terms.

*Example: The website should look clean and professional. Customers should see our logo at the top, easy navigation to product categories across the top, a search bar, and their shopping cart in the upper right corner.*

### 4.2 Hardware Interfaces
> What hardware will the software need to work with?

*Example: Standard computer, tablet, or phone with internet connection. No special scanners or card readers needed for customers.*

### 4.3 Software Interfaces
> What other software systems will this need to work with?

*Example: The website will connect to a payment company to process credit cards and PayPal payments.*

### 4.4 Communications Interfaces
> How will the software communicate with users or other systems?

*Example: Automatic emails for order confirmations, shipping updates, and password resets.*

---

## 5. Other Requirements

### 5.1 Legal Requirements
> Any legal or regulatory requirements?

*Example: Must follow privacy laws to protect customer information*

### 5.2 Standards Compliance
> Any industry standards to follow?

*Example: Website should be accessible to people with disabilities*

---

## 6. Prioritization of Requirements

> Use this table to indicate the priority of each requirement. This helps developers understand what must be included in the first version.

| Requirement ID | Description | Priority | Notes |
|----------------|-------------|----------|-------|
| FR-001 | User accounts | High | Essential for repeat customers |
| FR-002 | Product catalog | High | Core shopping feature |
| FR-003 | Shopping cart | High | Required for purchases |
| FR-004 | Mobile compatibility | Medium | Important but website works for now |
| NFR-001 | Fast loading | High | Customers will leave if slow |
| NFR-002 | Payment security | High | Critical for customer trust |

---

## 7. Future Enhancements (Optional)

> Features that might be added in future versions.

*Example:
- Customer reviews and ratings
- Wish lists for customers
- Gift wrapping options
- Advanced reporting for business insights*

---

## 8. Glossary

> Define all technical terms used in this document.

| Term | Definition |
|------|------------|
| [Term] | [Simple definition] |

---

## 9. Approval

> Section for stakeholders to sign off on the requirements.

**Prepared by:**
- Name: _________________________
- Title: _________________________
- Date: _________________________

**Reviewed by:**
- Name: _________________________
- Title: _________________________
- Date: _________________________

**Approved by:**
- Name: _________________________
- Title: _________________________
- Date: _________________________