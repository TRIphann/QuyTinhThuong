-- =====================================================
-- FILE MASTER: HƯỚNG DẪN CÀI ĐẶT HỆ THỐNG
-- HỆ THỐNG QUẢN LÝ QUỸ TÌNH THƯƠNG
-- =====================================================
-- 
-- QUAN TRỌNG: File này chỉ là hướng dẫn
-- 
-- Để cài đặt hệ thống, vui lòng chạy TỪNG FILE theo thứ tự:
-- 
-- CÁCH 1: Chạy từng file trong SSMS
--   1. Mở file: 01_CreateTables.sql   -> Execute (F5)
--   2. Mở file: 02_InsertData.sql     -> Execute (F5)
--   3. Mở file: 03_CreateSchedule.sql -> Execute (F5)
-- 
-- CÁCH 2: Sử dụng SQLCMD (Command Line)
--   sqlcmd -S . -i "01_CreateTables.sql"
--   sqlcmd -S . -i "02_InsertData.sql"
--   sqlcmd -S . -i "03_CreateSchedule.sql"
-- 
-- =====================================================
-- THÔNG TIN ĐĂNG NHẬP SAU KHI CÀI ĐẶT:
-- =====================================================
-- Database: QLQuyTinhThuong
-- 
-- Tài khoản:
-- - Admin:      admin / 123456789
-- - Staff:      staff1 / 123456789
-- - Accountant: accountant1 / 123456789
-- - Manager:    manager1 / 123456789
-- 
-- =====================================================

PRINT N'Vui lòng chạy từng file SQL theo thứ tự:'
PRINT N'1. 01_CreateTables.sql'
PRINT N'2. 02_InsertData.sql'
PRINT N'3. 03_CreateSchedule.sql'
