using BookWise.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BookWise.Infrastructure.Persistence.Configurations;

public class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.ToTable(
            "Entries",
            table =>
            {
                table.HasTrigger("TR_Entries_BlockNonLeafAccounts");
            });
        builder.HasKey(e => e.EntryId);

        builder.Property(e => e.Debit)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(e => e.Credit)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.HasCheckConstraint("CK_Entries_DebitCredit",
            "((Debit = 0 AND Credit > 0) OR (Credit = 0 AND Debit > 0))");

        builder.HasOne(e => e.Transaction)
            .WithMany(t => t.Entries)
            .HasForeignKey(e => e.TransactionId);

        builder.HasOne(e => e.Account)
            .WithMany(a => a.Entries)
            .HasForeignKey(e => e.AccountId);
    }
}
